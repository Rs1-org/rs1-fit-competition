export interface PollOption { publicId: string; label: string; votes: number; percentage: number; }
export interface Poll {
  publicId: string; matchPublicId: string; question: string; opensAt: string; closesAt: string;
  serverNow: string; isOpen: boolean; totalVotes: number; myOptionPublicId: string | null; options: PollOption[];
}
export interface PollInput { question: string; options: string[]; opensAt: string; closesAt: string; }
