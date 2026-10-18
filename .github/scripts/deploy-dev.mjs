import { resolve } from "node:path";
import { fileURLToPath } from "node:url";

export class DeploymentError extends Error {}

export function renderDeployUrl(hook, imageRepository, revision) {
  if (!/^[a-f0-9]{40}$/.test(revision ?? ""))
    throw new DeploymentError("A full Git commit SHA is required.");
  if (!/^docker\.io\/[a-z0-9_-]+\/[a-z0-9._-]+$/.test(imageRepository ?? ""))
    throw new DeploymentError("A Docker Hub image repository is required.");
  let url;
  try {
    url = new URL(hook);
  } catch {
    throw new DeploymentError("Render deploy hook is missing or invalid.");
  }
  if (
    url.protocol !== "https:" ||
    url.hostname !== "api.render.com" ||
    !url.pathname.startsWith("/deploy/srv-") ||
    !url.searchParams.get("key") ||
    url.username ||
    url.password ||
    url.hash
  )
    throw new DeploymentError(
      "Use the HTTPS deploy hook from this Render service.",
    );
  url.searchParams.set("imgURL", `${imageRepository}:sha-${revision}`);
  return url;
}

export async function waitForRevision(
  apiOrigin,
  revision,
  {
    fetchImpl = fetch,
    sleep = (ms) => new Promise((resolve) => setTimeout(resolve, ms)),
    attempts = 40,
    intervalMs = 15_000,
  } = {},
) {
  let origin;
  try {
    origin = new URL(apiOrigin);
  } catch {
    throw new DeploymentError("DEV API URL is invalid.");
  }
  if (
    origin.protocol !== "https:" ||
    origin.pathname !== "/" ||
    origin.search ||
    origin.hash ||
    origin.username ||
    origin.password ||
    !/^[a-f0-9]{40}$/.test(revision ?? "")
  )
    throw new DeploymentError(
      "DEV API must be an HTTPS origin and revision must be a full commit SHA.",
    );

  for (let attempt = 0; attempt < attempts; attempt++) {
    try {
      const response = await fetchImpl(new URL("/health", origin), {
        signal: AbortSignal.timeout(10_000),
        redirect: "error",
        cache: "no-store",
      });
      if (response.ok && (await response.json()).revision === revision) {
        const ready = await fetchImpl(new URL("/health/ready", origin), {
          signal: AbortSignal.timeout(10_000),
          redirect: "error",
          cache: "no-store",
        });
        if (ready.ok) return;
      }
    } catch {
      // Wakeup HTML, network errors and the previous healthy revision are not a completed deploy.
    }
    if (attempt + 1 < attempts) await sleep(intervalMs);
  }
  throw new DeploymentError(
    "Render did not serve the requested healthy revision before the deployment timeout.",
  );
}

export async function deployRender(
  env,
  { fetchImpl = fetch, ...pollOptions } = {},
) {
  const url = renderDeployUrl(
    env.RENDER_DEPLOY_HOOK_URL,
    env.IMAGE_REPOSITORY,
    env.GITHUB_SHA,
  );
  let response;
  try {
    response = await fetchImpl(url, {
      method: "POST",
      redirect: "error",
      signal: AbortSignal.timeout(30_000),
    });
  } catch {
    throw new DeploymentError(
      "Render deploy request failed. Check connectivity and the configured secret.",
    );
  }
  if (response.status !== 200 && response.status !== 202)
    throw new DeploymentError(
      `Render rejected the deploy request (HTTP ${response.status}).`,
    );
  console.log(
    "Render accepted the deploy request; waiting for the requested commit and database readiness.",
  );
  await waitForRevision(env.DEV_API_URL, env.GITHUB_SHA, {
    fetchImpl,
    ...pollOptions,
  });
  console.log(`Render is healthy at commit ${env.GITHUB_SHA}.`);
}

if (
  process.argv[1] &&
  resolve(process.argv[1]) === fileURLToPath(import.meta.url)
) {
  try {
    await deployRender(process.env);
  } catch (error) {
    console.error(
      error instanceof DeploymentError
        ? error.message
        : "DEV deployment failed; no credentials are logged.",
    );
    process.exitCode = 1;
  }
}
