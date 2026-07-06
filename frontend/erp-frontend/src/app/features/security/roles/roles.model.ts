/** Role read model (matches the backend RoleDto from ErpPlatform.Identity). */
export interface SecurityRole extends Record<string, unknown> {
  id: string;
  name: string;
  description?: string | null;
  permissions: string[];
  isActive: boolean;
  createdAt: string;
}
