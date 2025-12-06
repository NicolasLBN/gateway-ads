const express = require('express');
const router = express.Router();
const adsClient = require('../ads/adsClient');

// POST /api/recipe - Send recipe to PLC
router.post('/', async (req, res) => {
  try {
    const { name, ingredients } = req.body;

    if (!name || !ingredients || !Array.isArray(ingredients)) {
      return res.status(400).json({
        success: false,
        error: 'Invalid recipe format. Required: name and ingredients array',
      });
    }

    const connectionStatus = adsClient.getConnectionStatus();
    if (!connectionStatus.connected) {
      return res.status(503).json({
        success: false,
        error: 'Not connected to PLC. Please connect first.',
      });
    }

    const recipe = {
      name,
      ingredients: ingredients.map(ing => ({
        name: ing.name || '',
        quantity: parseFloat(ing.quantity) || 0,
        volume: parseFloat(ing.volume) || 0,
        molarMass: parseFloat(ing.molarMass) || 0,
      })),
    };

    await adsClient.writeRecipe(recipe);

    res.json({
      success: true,
      message: 'Recipe sent to PLC',
      recipe: recipe,
    });
  } catch (error) {
    res.status(500).json({
      success: false,
      error: error.message,
    });
  }
});

// POST /api/recipe/run - Start the process
router.post('/run', async (req, res) => {
  try {
    const connectionStatus = adsClient.getConnectionStatus();
    if (!connectionStatus.connected) {
      return res.status(503).json({
        success: false,
        error: 'Not connected to PLC. Please connect first.',
      });
    }

    await adsClient.startProcess();

    res.json({
      success: true,
      message: 'Process started',
    });
  } catch (error) {
    res.status(500).json({
      success: false,
      error: error.message,
    });
  }
});

// POST /api/recipe/reset - Reset the process
router.post('/reset', async (req, res) => {
  try {
    const connectionStatus = adsClient.getConnectionStatus();
    if (!connectionStatus.connected) {
      return res.status(503).json({
        success: false,
        error: 'Not connected to PLC. Please connect first.',
      });
    }

    await adsClient.resetProcess();

    res.json({
      success: true,
      message: 'Process reset',
    });
  } catch (error) {
    res.status(500).json({
      success: false,
      error: error.message,
    });
  }
});

module.exports = router;
