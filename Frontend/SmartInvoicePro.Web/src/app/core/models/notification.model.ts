export interface AppNotification {
  notificationId: string;
  title: string;
  message: string;
  type: string;
  referenceId?: string;
  isRead: boolean;
  createdDate: string;
}
