export type PaymentStatus = 'Pending' | 'Paid' | 'Expired';
export interface PremiumStatus { isPremium: boolean; premiumUntil: string | null; latestPaymentStatus: PaymentStatus | null; }
export interface PremiumPricing { monthlyPriceCents: number; currency: string; }
export interface CheckoutSession { url: string; }
export const premiumPlans = [1, 3, 12] as const;
export type PremiumPlanMonths = typeof premiumPlans[number];
