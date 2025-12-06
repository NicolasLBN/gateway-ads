const express = require('express');
const router = express.Router();
const adsClient = require('../ads/adsClient');

// POST /api/status/connect - Connect to PLC with ADS
router.post('/connect', async (req, res) => {
  try {
    const { amsNetId, amsPort } = req.body;

    const config = {};
    if (amsNetId) config.amsNetIdTarget = amsNetId;
    if (amsPort) config.amsPortTarget = parseInt(amsPort);

    const result = await adsClient.connect(config);

    res.json(result);
  } catch (error) {
    res.status(500).json({
      success: false,
      error: error.message,
    });
  }
});

// GET /api/status - Get current PLC status
router.get('/', async (req, res) => {
  try {
    const connectionStatus = adsClient.getConnectionStatus();

    if (!connectionStatus.connected) {
      return res.json({
        success: true,
        connected: false,
        message: 'Not connected to PLC',
      });
    }

    const machineData = await adsClient.readMachineData();
    const processData = await adsClient.readProcessData();

    res.json({
      success: true,
      connected: true,
      machine: machineData,
      process: processData,
      timestamp: new Date().toISOString(),
    });
  } catch (error) {
    res.status(500).json({
      success: false,
      error: error.message,
    });
  }
});

// POST /api/status/disconnect - Disconnect from PLC
router.post('/disconnect', (req, res) => {
  try {
    adsClient.disconnect();
    res.json({
      success: true,
      message: 'Disconnected from PLC',
    });
  } catch (error) {
    res.status(500).json({
      success: false,
      error: error.message,
    });
  }
});

module.exports = router;
