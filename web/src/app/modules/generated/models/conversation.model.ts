export interface Conversation {
  id: string;
  userId: string;
  organizationId: string;
  title: string;
  isArchived: boolean;
  isStarred: boolean;
  modelUsed: string;
  tokenCount: number;
  metadata: string;
  createdAt: Date | string;
  updatedAt: Date | string;
  deletedAt: Date | string;
}
