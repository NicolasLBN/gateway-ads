import { useQuery } from '@tanstack/react-query';
import { api } from '../services/api';

export function useFormulations(enabled = true) {
  return useQuery({
    queryKey: ['formulations'],
    queryFn: api.getFormulations,
    enabled,
  });
}

export function useFavorites(enabled = true) {
  return useQuery({
    queryKey: ['favorites'],
    queryFn: api.getFavorites,
    enabled,
  });
}

export function useReports(enabled = true) {
  return useQuery({
    queryKey: ['reports'],
    queryFn: api.getReports,
    enabled,
  });
}

export function useProcessStatus(enabled = true) {
  return useQuery({
    queryKey: ['process-status'],
    queryFn: api.getProcessStatus,
    enabled,
    refetchInterval: 2000,
  });
}
