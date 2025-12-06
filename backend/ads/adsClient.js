const ads = require('node-ads');

class ADSClient {
  constructor() {
    this.client = null;
    this.isConnected = false;
    this.config = {
      amsNetIdTarget: process.env.AMS_NET_ID || '127.0.0.1.1.1',
      amsPortTarget: parseInt(process.env.AMS_PORT || '851'),
      amsNetIdSource: null, // Auto-detect
      amsPortSource: null,  // Auto-detect
    };
    this.handles = {};
  }

  async connect(customConfig = {}) {
    try {
      const config = { ...this.config, ...customConfig };
      
      this.client = new ads.Client({
        host: config.amsNetIdTarget.split('.').slice(0, 4).join('.'),
        amsNetIdTarget: config.amsNetIdTarget,
        amsPortTarget: config.amsPortTarget,
      });

      await new Promise((resolve, reject) => {
        this.client.connect((error) => {
          if (error) {
            reject(error);
          } else {
            this.isConnected = true;
            resolve();
          }
        });
      });

      console.log('✅ ADS Client connected');
      return { success: true };
    } catch (error) {
      console.error('❌ ADS connection error:', error.message);
      this.isConnected = false;
      return { success: false, error: error.message };
    }
  }

  disconnect() {
    if (this.client) {
      this.client.end();
      this.isConnected = false;
      console.log('🔌 ADS Client disconnected');
    }
  }

  async readSymbol(symbolName) {
    if (!this.isConnected) {
      throw new Error('ADS client not connected');
    }

    return new Promise((resolve, reject) => {
      this.client.readSymbol(symbolName, (error, handle) => {
        if (error) {
          reject(error);
        } else {
          resolve(handle.value);
        }
      });
    });
  }

  async writeSymbol(symbolName, value) {
    if (!this.isConnected) {
      throw new Error('ADS client not connected');
    }

    return new Promise((resolve, reject) => {
      this.client.writeSymbol(symbolName, value, (error) => {
        if (error) {
          reject(error);
        } else {
          resolve({ success: true });
        }
      });
    });
  }

  async readMachineData() {
    try {
      const data = {
        motorTemperature: await this.readSymbol('GVL_Machine.MotorTemperature'),
        oilPressure: await this.readSymbol('GVL_Machine.OilPressure'),
        motorSpeed: await this.readSymbol('GVL_Machine.MotorSpeed'),
        tempWarning: await this.readSymbol('GVL_Machine.TempWarning'),
        pressureWarning: await this.readSymbol('GVL_Machine.PressureWarning'),
        speedWarning: await this.readSymbol('GVL_Machine.SpeedWarning'),
      };
      return data;
    } catch (error) {
      console.error('Error reading machine data:', error.message);
      throw error;
    }
  }

  async readProcessData() {
    try {
      const data = {
        currentStep: await this.readSymbol('GVL_Process.CurrentStep'),
        stepName: await this.readSymbol('GVL_Process.StepName'),
        progress: await this.readSymbol('GVL_Process.Progress'),
        stepProgress: await this.readSymbol('GVL_Process.StepProgress'),
        stepTime_s: await this.readSymbol('GVL_Process.StepTime_s'),
        totalTime_s: await this.readSymbol('GVL_Process.TotalTime_s'),
        errorCode: await this.readSymbol('GVL_Process.ErrorCode'),
        errorText: await this.readSymbol('GVL_Process.ErrorText'),
        processDone: await this.readSymbol('GVL_Process.ProcessDone'),
      };
      return data;
    } catch (error) {
      console.error('Error reading process data:', error.message);
      throw error;
    }
  }

  async writeRecipe(recipe) {
    try {
      await this.writeSymbol('GVL_Recipe.RecipeName', recipe.name);
      await this.writeSymbol('GVL_Recipe.NumIngredients', recipe.ingredients.length);

      for (let i = 0; i < recipe.ingredients.length; i++) {
        const ingredient = recipe.ingredients[i];
        await this.writeSymbol(`GVL_Recipe.IngredientName[${i + 1}]`, ingredient.name);
        await this.writeSymbol(`GVL_Recipe.IngredientQuantity[${i + 1}]`, ingredient.quantity);
        await this.writeSymbol(`GVL_Recipe.IngredientVolume[${i + 1}]`, ingredient.volume);
        await this.writeSymbol(`GVL_Recipe.IngredientMolarMass[${i + 1}]`, ingredient.molarMass);
      }

      return { success: true };
    } catch (error) {
      console.error('Error writing recipe:', error.message);
      throw error;
    }
  }

  async startProcess() {
    try {
      await this.writeSymbol('GVL_Command.StartProcess', true);
      // Wait a bit and reset the command
      setTimeout(async () => {
        await this.writeSymbol('GVL_Command.StartProcess', false);
      }, 500);
      return { success: true };
    } catch (error) {
      console.error('Error starting process:', error.message);
      throw error;
    }
  }

  async resetProcess() {
    try {
      await this.writeSymbol('GVL_Command.ResetProcess', true);
      setTimeout(async () => {
        await this.writeSymbol('GVL_Command.ResetProcess', false);
      }, 500);
      return { success: true };
    } catch (error) {
      console.error('Error resetting process:', error.message);
      throw error;
    }
  }

  getConnectionStatus() {
    return {
      connected: this.isConnected,
      config: this.config,
    };
  }
}

// Singleton instance
const adsClient = new ADSClient();

module.exports = adsClient;
