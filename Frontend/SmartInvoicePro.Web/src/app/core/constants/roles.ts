export const ROLES = {
  Admin: 'Admin',
  Staff: 'Staff',
} as const;

export type Role = (typeof ROLES)[keyof typeof ROLES];
