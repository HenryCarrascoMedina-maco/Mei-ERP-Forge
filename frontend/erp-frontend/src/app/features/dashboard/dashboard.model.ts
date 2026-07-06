import { KpiStatus, KpiTrend } from '@erp-platform/core';

export interface DashboardKpi {
  key: string;
  label: string;
  value: number;
  trend: KpiTrend;
  percentage: number;
  status: KpiStatus;
}

export interface StatusSummary {
  status: string;
  count: number;
}

export interface RecentItem {
  title: string;
  subtitle: string;
  status: string;
  date: string;
}

/** Mirrors the backend `DashboardStatsDto`. */
export interface DashboardStats {
  kpis: DashboardKpi[];
  statusSummary: StatusSummary[];
  recent: RecentItem[];
}
