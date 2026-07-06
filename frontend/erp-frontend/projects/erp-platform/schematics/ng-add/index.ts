import { chain, Rule, SchematicContext, SchematicsException, Tree } from '@angular-devkit/schematics';
import { addRootProvider } from '@schematics/angular/utility';
import { getWorkspace } from '@schematics/angular/utility/workspace';

interface Options {
  project?: string;
  apiUrl?: string;
}

/** Coarse idempotency check: has the platform already been wired into a *.config.ts? */
function alreadyConfigured(tree: Tree): boolean {
  let found = false;
  tree.visit((path, entry) => {
    if (found || !entry) return;
    if (path.endsWith('.config.ts') && entry.content.toString().includes('provideErpPlatform')) {
      found = true;
    }
  });
  return found;
}

async function resolveProject(tree: Tree, requested?: string): Promise<string> {
  const workspace = await getWorkspace(tree);
  if (requested && workspace.projects.has(requested)) return requested;
  for (const [name, project] of workspace.projects) {
    if (project.extensions['projectType'] === 'application') return name;
  }
  throw new SchematicsException('No application project found to configure.');
}

/**
 * `ng add @erp-platform/core` — wires the platform into the host application's root providers:
 * `provideErpPlatform({ apiUrl })`, HttpClient with the platform interceptors, and async animations.
 * Idempotent: a no-op when the platform is already configured.
 */
export function ngAdd(options: Options): Rule {
  return async (tree: Tree, context: SchematicContext) => {
    if (alreadyConfigured(tree)) {
      context.logger.info('@erp-platform/core is already configured — nothing to do.');
      return chain([]);
    }

    const project = await resolveProject(tree, options.project);
    const apiUrl = options.apiUrl ?? '/api';
    context.logger.info(`Configuring @erp-platform/core in project "${project}" (apiUrl: ${apiUrl}).`);

    const provideErp: any = ({ code, external }: any) =>
      code`${external('provideErpPlatform', '@erp-platform/core')}({ apiUrl: ${JSON.stringify(apiUrl)} })`;

    const provideHttp: any = ({ code, external }: any) =>
      code`${external('provideHttpClient', '@angular/common/http')}(${external('withInterceptors', '@angular/common/http')}([${external('correlationIdInterceptor', '@erp-platform/core')}, ${external('authInterceptor', '@erp-platform/core')}, ${external('loadingInterceptor', '@erp-platform/core')}, ${external('errorInterceptor', '@erp-platform/core')}]))`;

    const provideAnims: any = ({ code, external }: any) =>
      code`${external('provideAnimationsAsync', '@angular/platform-browser/animations/async')}()`;

    return chain([
      addRootProvider(project, provideErp),
      addRootProvider(project, provideHttp),
      addRootProvider(project, provideAnims),
    ]);
  };
}
