import { SectionTab } from '../../shared/components/section-tabs/section-tabs';

export function statisticsTabs(isPremium: boolean): SectionTab[] {
  return [
    { label: 'Team statistics', route: '/statistics' },
    { label: 'Charts', route: '/statistics/charts' },
    { label: 'Advanced', route: '/statistics/advanced', locked: !isPremium },
  ];
}
