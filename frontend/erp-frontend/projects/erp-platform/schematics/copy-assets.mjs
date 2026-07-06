// Copies schematic JSON assets (collection, option schemas, manifest schema, examples) into the
// built package at dist/erp-platform/schematics, next to the compiled factories.
import { cpSync, mkdirSync } from 'node:fs';
import { join, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const dist = join(here, '..', '..', '..', 'dist', 'erp-platform', 'schematics');

const assets = [
  ['collection.json', 'collection.json'],
  ['module/schema.json', 'module/schema.json'],
  ['ng-add/schema.json', 'ng-add/schema.json'],
  ['schemas', 'schemas'],
  ['examples', 'examples'],
];

for (const [from, to] of assets) {
  const dest = join(dist, to);
  mkdirSync(dirname(dest), { recursive: true });
  cpSync(join(here, from), dest, { recursive: true });
}
console.log('Copied schematic assets to dist/erp-platform/schematics');
