#!/usr/bin/env node
import { readFileSync, existsSync, readdirSync } from 'node:fs';
import { resolve, join, basename } from 'node:path';
import Ajv2020, { type ErrorObject } from 'ajv/dist/2020';
import addFormats from 'ajv-formats';

// __dirname = <schematics>/validate; schema + examples live under <schematics>/.
const SCHEMATICS_ROOT = resolve(__dirname, '..');
const SCHEMA_PATH = join(SCHEMATICS_ROOT, 'schemas', 'module-manifest.schema.json');
const EXAMPLES_DIR = join(SCHEMATICS_ROOT, 'examples');

/**
 * Validates one or more `*.module.json` manifests against the ERP Platform manifest schema.
 * Usage: `node dist/validate/index.js [file ...]` (defaults to the bundled examples).
 */
function loadJson(path: string): unknown {
  return JSON.parse(readFileSync(path, 'utf8'));
}

function formatErrors(errors: ErrorObject[] | null | undefined): string {
  return (errors ?? [])
    .map((e) => `   - ${e.instancePath || '(root)'} ${e.message}${e.params ? ' ' + JSON.stringify(e.params) : ''}`)
    .join('\n');
}

function resolveTargets(args: string[]): string[] {
  if (args.length > 0) {
    return args.map((a) => resolve(process.cwd(), a));
  }
  if (!existsSync(EXAMPLES_DIR)) return [];
  return readdirSync(EXAMPLES_DIR)
    .filter((f) => f.endsWith('.module.json'))
    .map((f) => join(EXAMPLES_DIR, f));
}

function main(): void {
  const ajv = new Ajv2020({ allErrors: true, strict: false });
  addFormats(ajv);

  const schema = loadJson(SCHEMA_PATH) as object;
  const validate = ajv.compile(schema);

  const targets = resolveTargets(process.argv.slice(2));
  if (targets.length === 0) {
    console.error('No manifests found to validate.');
    process.exit(1);
  }

  let failures = 0;
  for (const file of targets) {
    if (!existsSync(file)) {
      console.error(`✗ ${file} — file not found`);
      failures++;
      continue;
    }
    const data = loadJson(file);
    const ok = validate(data);
    if (ok) {
      console.log(`✓ ${basename(file)} — valid (schemaVersion ${(data as { schemaVersion?: string }).schemaVersion})`);
    } else {
      failures++;
      console.error(`✗ ${basename(file)} — invalid\n${formatErrors(validate.errors)}`);
    }
  }

  console.log(`\n${targets.length - failures}/${targets.length} manifest(s) valid.`);
  process.exit(failures > 0 ? 1 : 0);
}

main();
