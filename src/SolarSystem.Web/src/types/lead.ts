export type LeadStatus = 'new' | 'contacting' | 'proposal_sent' | 'won' | 'lost';
export type LeadType = 'residential' | 'commercial' | 'industrial' | 'rural';
export type LeadSource = 'website' | 'referral' | 'social_media' | 'ads' | 'other';

export interface Lead {
  id: string;
  name: string;
  phone: string;
  email?: string;
  city?: string;
  uf?: string;
  leadType: LeadType;
  leadSource: LeadSource;
  status: LeadStatus;
  notes?: string;
  consumptionEstimate?: number;
  createdAt: string;
  updatedAt?: string;
}

export interface PaginatedResult<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface GetLeadsParams {
  page?: number;
  pageSize?: number;
  status?: string;
  uf?: string;
  type?: string;
  search?: string;
}

export interface CreateLeadRequest {
  name: string;
  phone: string;
  email?: string;
  city?: string;
  uf?: string;
  leadType?: string;
  leadSource?: string;
  consumptionEstimate?: number;
  notes?: string;
}

export interface UpdateLeadRequest {
  name?: string;
  phone?: string;
  email?: string;
  city?: string;
  uf?: string;
  leadType?: string;
  leadSource?: string;
  consumptionEstimate?: number;
}

export interface AddNoteRequest {
  note: string;
}

export interface ChangeStatusRequest {
  status: LeadStatus;
}
