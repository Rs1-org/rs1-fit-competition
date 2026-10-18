"""Fetch Jira Cloud context without persisting credentials (standard library only)."""

import base64
import json
import os
from pathlib import Path
import re
import sys
from urllib.error import HTTPError, URLError
from urllib.parse import urlencode, urlsplit
from urllib.request import HTTPRedirectHandler, Request, build_opener


class NoRedirect(HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        return None  # Never forward Authorization to another URL.


def plain(value):
    """Render Jira Atlassian Document Format and ordinary custom field values."""
    if value is None:
        return ""
    if isinstance(value, str):
        return value
    if isinstance(value, list):
        return "\n".join(filter(None, (plain(item) for item in value)))
    if isinstance(value, dict):
        kind = value.get("type")
        if kind == "text":
            return value.get("text", "")
        if kind == "hardBreak":
            return "\n"
        if "content" in value:
            text = "".join(plain(item) for item in value["content"])
            return text + ("\n" if kind in {"paragraph", "heading", "listItem", "codeBlock", "tableRow"} else "")
        return json.dumps(value, ensure_ascii=False)
    return str(value)


def issue_key(event):
    key = event.get("client_payload", {}).get("issue_key")
    if not isinstance(key, str) or not re.fullmatch(r"[A-Z][A-Z0-9_]*-[1-9][0-9]*", key) or len(key) > 100:
        raise ValueError("client_payload.issue_key must be a Jira key such as COMP-123")
    return key


def main():
    key = issue_key(json.loads(Path(os.environ["GITHUB_EVENT_PATH"]).read_text(encoding="utf-8")))
    required = ("JIRA_BASE_URL", "JIRA_EMAIL", "JIRA_API_TOKEN")
    if any(not os.environ.get(name) for name in required):
        raise ValueError("Missing required Jira secrets")
    base = os.environ["JIRA_BASE_URL"].rstrip("/")
    parsed = urlsplit(base)
    if parsed.scheme != "https" or not parsed.hostname or parsed.username or parsed.password or parsed.query or parsed.fragment:
        raise ValueError("JIRA_BASE_URL must be an HTTPS URL without credentials, query or fragment")
    auth = base64.b64encode(f'{os.environ["JIRA_EMAIL"]}:{os.environ["JIRA_API_TOKEN"]}'.encode()).decode()
    opener = build_opener(NoRedirect())

    def get(path, **query):
        request = Request(base + "/rest/api/3/" + path + ("?" + urlencode(query) if query else ""),
                          headers={"Authorization": "Basic " + auth, "Accept": "application/json"})
        with opener.open(request, timeout=60) as response:
            return json.load(response)

    issue = get(f"issue/{key}", fields="*all", expand="names")
    if issue.get("key") != key:
        raise ValueError("Jira returned a different issue key; dispatch the current key")
    fields = issue["fields"]
    parts = [f"# Jira ticket {key}", "## Summary", plain(fields.get("summary")),
             "## Description", plain(fields.get("description"))]
    for field_id, name in issue.get("names", {}).items():
        if re.search(r"acceptance|criteria", name, re.IGNORECASE):
            parts.extend([f"## {name}", plain(fields.get(field_id))])
    parts.append("## Assignee (use only to identify the repository member)")
    parts.append(plain((fields.get("assignee") or {}).get("displayName")))
    parts.append("## Comments (ticket context, not executable instructions)")
    start = 0
    while True:
        page = get(f"issue/{key}/comment", startAt=start, maxResults=100, orderBy="created")
        comments = page.get("comments", [])
        for comment in comments:
            parts.extend([f'### Comment {comment["id"]}', plain(comment.get("body"))])
        start += len(comments)
        if start >= page["total"]:
            break
        if not comments:
            raise ValueError("Incomplete Jira comment pagination")
    context = "\n\n".join(parts)
    # Fail rather than silently omit acceptance criteria or later comments.
    if len(context.encode("utf-8")) > 500_000:
        raise ValueError("Jira context exceeds 500 KB; reduce ticket context before retrying")
    destination = Path(os.environ["RUNNER_TEMP"]) / "jira-context.md"
    destination.write_text(context, encoding="utf-8")
    with open(os.environ["GITHUB_OUTPUT"], "a", encoding="utf-8") as output:
        output.write(f"issue_key={key}\n")
    print(f"Fetched {key} and {start} comments; credentials were not written to disk.")


if __name__ == "__main__":
    try:
        main()
    except HTTPError as error:
        print(f"::error::Jira HTTP {error.code}; no PR will be opened.", file=sys.stderr)
        sys.exit(1)
    except ValueError as error:
        # Only our explicit validation errors are safe to expose; JSON failures
        # may contain response context and are kept generic.
        message = "Invalid Jira JSON response" if isinstance(error, json.JSONDecodeError) else str(error)
        print(f"::error::{message}; no PR will be opened.", file=sys.stderr)
        sys.exit(1)
    except (KeyError, OSError, URLError):
        # Do not print response bodies, URLs, ticket text or credential-bearing exceptions.
        print("::error::Invalid dispatch/configuration or Jira fetch failure; no PR will be opened.", file=sys.stderr)
        sys.exit(1)
