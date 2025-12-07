const ads = require('node-ads');

class ADSClient {
  constructor() {
    this.client = null;
    this.isConnected = false;
    this.config = {
      amsNetIdTarget: process.env.AMS_NET_ID || '199.4.42.250.1.1',
      amsPortTarget: parseInt(process.env.AMS_PORT || '851'),
      host: process.env.ADS_HOST || '127.0.0.1', // Adresse IP locale
    };
  }

  connect(customConfig = {}) {
    return new Promise((resolve, reject) => {
      const config = { ...this.config, ...customConfig };
      console.log('ADS Client connecting with config:', config);

      this.client = ads.connect({
        host: config.host,
        amsNetIdTarget: config.amsNetIdTarget,
        amsPortTarget: config.amsPortTarget,
      }, (err) => {
        if (err) {
          this.isConnected = false;
          console.error('❌ ADS connection error:', err.message || err);
          reject(err);
        } else {
          this.isConnected = true;
          console.log('✅ ADS Client connected');
          resolve({ success: true });
        }
      });

      console.log('ADS Client connection initiated');

      // Gestion d'erreurs asynchrones
      this.client.on('error', (err) => {
        console.error('ADS Client error:', err);
      });
    });
  }

  disconnect() {
    if (this.client) {
      this.client.end();
      this.isConnected = false;
      console.log('🔌 ADS Client disconnected');
    }
  }

  readSymbol(symbolName) {
    if (!this.isConnected) {
      return Promise.reject(new Error('ADS client not connected'));
    }

    return new Promise((resolve, reject) => {
      this.client.read(symbolName, (err, val) => {
        if (err) reject(err);
        else resolve(val.value);
      });
    });
  }

  writeSymbol(symbolName, value) {
    if (!this.isConnected) {
      return Promise.reject(new Error('ADS client not connected'));
    }

    return new Promise((resolve, reject) => {
      this.client.write(symbolName, value, (err) => {
        if (err) reject(err);
        else resolve({ success: true });
      });
    });
  }

  async readMachineData() {
    try {
      return {
        motorTemperature: await this.readSymbol('GVL_Machine.MotorTemperature'),
        oilPressure: await this.readSymbol('GVL_Machine.OilPressure'),
        motorSpeed: await this.readSymbol('GVL_Machine.MotorSpeed'),
        tempWarning: await this.readSymbol('GVL_Machine.TempWarning'),
        pressureWarning: await this.readSymbol('GVL_Machine.PressureWarning'),
        speedWarning: await this.readSymbol('GVL_Machine.SpeedWarning'),
      };
    } catch (err) {
      console.error('Error reading machine data:', err.message || err);
      throw err;
    }
  }

  async readProcessData() {
    try {
      return {
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
    } catch (err) {
      console.error('Error reading process data:', err.message || err);
      throw err;
    }
  }

  async writeRecipe(recipe) {
    try {
      await this.writeSymbol('GVL_Recipe.RecipeName', recipe.name);
      await this.writeSymbol('GVL_Recipe.NumIngredients', recipe.ingredients.length);

      for (let i = 0; i < recipe.ingredients.length; i++) {
        const ing = recipe.ingredients[i];
        await this.writeSymbol(`GVL_Recipe.IngredientName[${i + 1}]`, ing.name);
        await this.writeSymbol(`GVL_Recipe.IngredientQuantity[${i + 1}]`, ing.quantity);
        await this.writeSymbol(`GVL_Recipe.IngredientVolume[${i + 1}]`, ing.volume);
        await this.writeSymbol(`GVL_Recipe.IngredientMolarMass[${i + 1}]`, ing.molarMass);
      }

      return { success: true };
    } catch (err) {
      console.error('Error writing recipe:', err.message || err);
      throw err;
    }
  }

  async startProcess() {
    try {
      await this.writeSymbol('GVL_Command.StartProcess', true);
      setTimeout(async () => {
        try { await this.writeSymbol('GVL_Command.StartProcess', false); }
        catch (err) { console.error('Error resetting StartProcess flag:', err.message || err); }
      }, 500);
      return { success: true };
    } catch (err) {
      console.error('Error starting process:', err.message || err);
      throw err;
    }
  }

  async resetProcess() {
    try {
      await this.writeSymbol('GVL_Command.ResetProcess', true);
      setTimeout(async () => {
        try { await this.writeSymbol('GVL_Command.ResetProcess', false); }
        catch (err) { console.error('Error resetting ResetProcess flag:', err.message || err); }
      }, 500);
      return { success: true };
    } catch (err) {
      console.error('Error resetting process:', err.message || err);
      throw err;
    }
  }

  getConnectionStatus() {
    return {
      connected: this.isConnected,
      config: this.config,
    };
  }
}

// Singleton
const adsClient = new ADSClient();
module.exports = adsClient;
