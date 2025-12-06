const express = require('express');
const router = express.Router();
const fs = require('fs').promises;
const path = require('path');
const { v4: uuidv4 } = require('uuid');
const { generatePDF } = require('../pdf/pdfGenerator');

const HISTORY_FILE = path.join(__dirname, '../data/history.json');
const REPORTS_DIR = path.join(__dirname, '../reports');

// Ensure directories exist
async function ensureDirectories() {
  const dataDir = path.join(__dirname, '../data');
  try {
    await fs.mkdir(dataDir, { recursive: true });
    await fs.mkdir(REPORTS_DIR, { recursive: true });
  } catch (error) {
    console.error('Error creating directories:', error);
  }
}

// Initialize
ensureDirectories();

// Load history from file
async function loadHistory() {
  try {
    const data = await fs.readFile(HISTORY_FILE, 'utf8');
    return JSON.parse(data);
  } catch (error) {
    // File doesn't exist or is invalid, return empty array
    return [];
  }
}

// Save history to file
async function saveHistory(history) {
  try {
    await fs.writeFile(HISTORY_FILE, JSON.stringify(history, null, 2));
  } catch (error) {
    console.error('Error saving history:', error);
    throw error;
  }
}

// GET /api/history - Get all recipe history
router.get('/', async (req, res) => {
  try {
    const history = await loadHistory();
    res.json({
      success: true,
      history: history,
    });
  } catch (error) {
    res.status(500).json({
      success: false,
      error: error.message,
    });
  }
});

// POST /api/history - Add a new recipe report
router.post('/', async (req, res) => {
  try {
    const { recipeName, machineName, products, steps } = req.body;

    if (!recipeName || !products || !steps) {
      return res.status(400).json({
        success: false,
        error: 'Invalid report format. Required: recipeName, products, steps',
      });
    }

    const history = await loadHistory();
    
    const report = {
      id: `report-${uuidv4()}`,
      recipeName,
      machineName: machineName || 'Unknown Machine',
      date: new Date().toISOString(),
      products,
      steps,
      createdAt: new Date().toISOString(),
    };

    // Generate PDF
    const pdfPath = await generatePDF(report);
    report.pdfPath = pdfPath;
    report.pdfUrl = `/reports/${path.basename(pdfPath)}`;

    history.push(report);
    await saveHistory(history);

    res.json({
      success: true,
      report: report,
    });
  } catch (error) {
    console.error('Error creating report:', error);
    res.status(500).json({
      success: false,
      error: error.message,
    });
  }
});

// GET /api/history/:id - Get specific report
router.get('/:id', async (req, res) => {
  try {
    const history = await loadHistory();
    const report = history.find(r => r.id === req.params.id);

    if (!report) {
      return res.status(404).json({
        success: false,
        error: 'Report not found',
      });
    }

    res.json({
      success: true,
      report: report,
    });
  } catch (error) {
    res.status(500).json({
      success: false,
      error: error.message,
    });
  }
});

// GET /api/history/:id/pdf - Download PDF
router.get('/:id/pdf', async (req, res) => {
  try {
    const history = await loadHistory();
    const report = history.find(r => r.id === req.params.id);

    if (!report || !report.pdfPath) {
      return res.status(404).json({
        success: false,
        error: 'PDF not found',
      });
    }

    // Check if file exists
    try {
      await fs.access(report.pdfPath);
      res.download(report.pdfPath);
    } catch (error) {
      return res.status(404).json({
        success: false,
        error: 'PDF file not found on disk',
      });
    }
  } catch (error) {
    res.status(500).json({
      success: false,
      error: error.message,
    });
  }
});

module.exports = router;
