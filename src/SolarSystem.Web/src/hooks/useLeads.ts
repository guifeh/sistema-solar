import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api from '../lib/api';
import type { 
  Lead, 
  PaginatedResult, 
  GetLeadsParams, 
  CreateLeadRequest, 
  UpdateLeadRequest, 
  AddNoteRequest, 
  ChangeStatusRequest 
} from '../types/lead';

// Keys for React Query cache
export const leadKeys = {
  all: ['leads'] as const,
  lists: () => [...leadKeys.all, 'list'] as const,
  list: (filters: GetLeadsParams) => [...leadKeys.lists(), { filters }] as const,
  details: () => [...leadKeys.all, 'detail'] as const,
  detail: (id: string) => [...leadKeys.details(), id] as const,
};

// Fetchers
const fetchLeads = async (params: GetLeadsParams): Promise<PaginatedResult<Lead>> => {
  const { data } = await api.get<PaginatedResult<Lead>>('/leads', { params });
  return data;
};

const fetchLeadById = async (id: string): Promise<Lead> => {
  const { data } = await api.get<Lead>(`/leads/${id}`);
  return data;
};

// Hooks
export function useLeads(filters: GetLeadsParams) {
  return useQuery({
    queryKey: leadKeys.list(filters),
    queryFn: () => fetchLeads(filters),
    placeholderData: (previousData) => previousData, // keep previous data while fetching new page
  });
}

export function useLead(id: string) {
  return useQuery({
    queryKey: leadKeys.detail(id),
    queryFn: () => fetchLeadById(id),
    enabled: !!id,
  });
}

export function useCreateLead() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (newLead: CreateLeadRequest) => {
      const { data } = await api.post<Lead>('/leads', newLead);
      return data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: leadKeys.lists() });
    },
  });
}

export function useUpdateLead() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, data }: { id: string; data: UpdateLeadRequest }) => {
      const response = await api.put<Lead>(`/leads/${id}`, data);
      return response.data;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: leadKeys.detail(variables.id) });
      queryClient.invalidateQueries({ queryKey: leadKeys.lists() });
    },
  });
}

export function useAddLeadNote() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, note }: { id: string; note: string }) => {
      await api.post(`/leads/${id}/notes`, { note } as AddNoteRequest);
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: leadKeys.detail(variables.id) });
    },
  });
}

export function useChangeLeadStatus() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({ id, status }: { id: string; status: string }) => {
      await api.post(`/leads/${id}/status`, { status } as ChangeStatusRequest);
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: leadKeys.detail(variables.id) });
      queryClient.invalidateQueries({ queryKey: leadKeys.lists() });
    },
  });
}
