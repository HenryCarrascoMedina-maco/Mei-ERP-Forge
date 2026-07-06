/*
 * Public API of @erp-platform/core — the reusable ERP framework library.
 * This file IS the public contract. Anything not re-exported here is internal.
 */

// Runtime configuration (host wiring)
export * from './lib/config/erp-platform.config';

// Shared surface: models, services, interceptors, components, directives, guards,
// validators, pipes, configs, utils (aggregated by the shared barrel).
export * from './lib';
