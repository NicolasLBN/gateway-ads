import { create } from 'zustand';

export const useStore = create((set) => ({
  // Machine state
  selectedMachine: null,
  machines: [],
  setSelectedMachine: (machine) => set({ selectedMachine: machine }),
  setMachines: (machines) => set({ machines }),

  // Connection state
  isConnected: false,
  setIsConnected: (isConnected) => set({ isConnected }),

  // Real-time data
  machineData: {
    motorTemperature: 0,
    oilPressure: 0,
    motorSpeed: 0,
    tempWarning: false,
    pressureWarning: false,
    speedWarning: false,
  },
  processData: {
    currentStep: 0,
    stepName: 'Idle',
    progress: 0,
    stepProgress: 0,
    stepTime_s: 0,
    totalTime_s: 0,
    errorCode: 0,
    errorText: '',
    processDone: false,
  },
  setMachineData: (data) => set({ machineData: data }),
  setProcessData: (data) => set({ processData: data }),

  // Current recipe
  currentRecipe: null,
  setCurrentRecipe: (recipe) => set({ currentRecipe: recipe }),

  // Process history for charting
  processHistory: [],
  addProcessHistory: (data) =>
    set((state) => ({
      processHistory: [...state.processHistory, data].slice(-100), // Keep last 100 points
    })),
  clearProcessHistory: () => set({ processHistory: [] }),

  // Recipe history
  recipeHistory: [],
  setRecipeHistory: (history) => set({ recipeHistory: history }),
}));
