export const ASSIGNABLE_USER_ROLES = ['Admin', 'Moderator', 'User'] as const;
export const USER_FILTER_ROLES = [...ASSIGNABLE_USER_ROLES, 'Manager'] as const;

export interface AdminUser {
  publicId: string;
  email: string;
  firstName: string;
  lastName: string;
  isActive: boolean;
  registeredAt: string | null;
  roles: string[];
  isSelf: boolean;
}

export interface UserFilter {
  search?: string;
  role?: string;
  isActive?: boolean;
  registeredFrom?: string;
  registeredTo?: string;
}
