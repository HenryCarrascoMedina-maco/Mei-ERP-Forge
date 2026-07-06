/** User read model (matches the backend UserDto from ErpPlatform.Identity). */
export interface SecurityUser extends Record<string, unknown> {
  id: string;
  userName: string;
  email: string;
  displayName?: string | null;
  isActive: boolean;
  /** Role names (display). */
  roles: string[];
  /** Role ids (edit form selector). */
  roleIds: string[];
  createdAt: string;
}
