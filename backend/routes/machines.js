const express = require('express');
const router = express.Router();

// Mock machine data - in production, this could come from a database
const machines = [
  {
    id: 'machine-1',
    name: 'Mixing Unit A',
    amsNetId: '127.0.0.1.1.1',
    amsPort: 851,
    location: 'Production Line 1',
  },
  {
    id: 'machine-2',
    name: 'Mixing Unit B',
    amsNetId: '127.0.0.1.1.2',
    amsPort: 852,
    location: 'Production Line 2',
  },
  {
    id: 'machine-3',
    name: 'Dosing Unit A',
    amsNetId: '127.0.0.1.1.3',
    amsPort: 853,
    location: 'Production Line 1',
  },
];

// GET /api/machines - Get list of all machines
router.get('/', (req, res) => {
  res.json({
    success: true,
    machines: machines,
  });
});

// GET /api/machines/:id - Get specific machine
router.get('/:id', (req, res) => {
  const machine = machines.find(m => m.id === req.params.id);
  
  if (!machine) {
    return res.status(404).json({
      success: false,
      error: 'Machine not found',
    });
  }

  res.json({
    success: true,
    machine: machine,
  });
});

module.exports = router;
