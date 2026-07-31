import { useQuery } from '@tanstack/react-query';
import { api } from '../services/api';

export { useProcessStatus } from './useProcessStatus';

export function useRecipes(enabled = true) {
  return useQuery({
    queryKey: ['recipes'],
    queryFn: api.getRecipes,
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
