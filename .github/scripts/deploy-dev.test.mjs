import { test } from "node:test";
import assert from "node:assert/strict";
import {
  deployRender,
  renderDeployUrl,
  waitForRevision,
} from "./deploy-dev.mjs";

const revision = "a".repeat(40);
const hook = "https://api.render.com/deploy/srv-test?key=test-only";
const image = "docker.io/dinofejzagic/competition-dev";
const api = "https://competition-dev-api.onrender.com";
const response = (body, status = 200) =>
  new Response(JSON.stringify(body), { status });

test("deploy hook targets the immutable image tag without losing its authentication key", () => {
  const url = renderDeployUrl(hook, image, revision);
  assert.equal(url.searchParams.get("key"), "test-only");
  assert.equal(url.searchParams.get("imgURL"), `${image}:sha-${revision}`);
});

test("rejects non-Render hooks and malformed image/revision before sending secrets", () => {
  for (const bad of [
    "http://api.render.com/deploy/srv-x?key=x",
    "https://attacker.invalid/deploy/srv-x?key=x",
  ])
    assert.throws(() => renderDeployUrl(bad, image, revision));
  assert.throws(() => renderDeployUrl(hook, `${image};echo`, revision));
  assert.throws(() => renderDeployUrl(hook, image, "main"));
});

test("old healthy revision and unavailable DB do not falsely complete deployment", async () => {
  const results = [
    response({ revision: "b".repeat(40) }),
    response({ revision }),
    response({}, 503),
    response({ revision }),
    response({ status: "ready" }),
  ];
  let sleeps = 0;
  await waitForRevision(api, revision, {
    fetchImpl: async () => results.shift(),
    sleep: async () => {
      sleeps++;
    },
    attempts: 3,
  });
  assert.equal(results.length, 0);
  assert.equal(sleeps, 2);
});

test("cold-start HTML and network errors can recover on the next attempt", async () => {
  let calls = 0;
  await waitForRevision(api, revision, {
    fetchImpl: async () => {
      calls++;
      if (calls === 1) throw new Error("network");
      if (calls === 2) return new Response("<html>Starting</html>");
      return response({ revision });
    },
    sleep: async () => {},
    attempts: 3,
  });
  assert.equal(calls, 4);
});

test("times out when the new revision never becomes healthy", async () => {
  await assert.rejects(
    waitForRevision(api, revision, {
      fetchImpl: async () => response({ revision: "old" }),
      sleep: async () => {},
      attempts: 2,
    }),
    /requested healthy revision/,
  );
});

test("Render rejection reports status without exposing the secret deploy hook", async () => {
  await assert.rejects(
    deployRender(
      {
        RENDER_DEPLOY_HOOK_URL: hook,
        IMAGE_REPOSITORY: image,
        GITHUB_SHA: revision,
        DEV_API_URL: api,
      },
      { fetchImpl: async () => response({}, 401) },
    ),
    (error) =>
      error.message.includes("HTTP 401") &&
      !error.message.includes("test-only"),
  );
});
