const fs = require('fs').promises;
const path = require('path');
const puppeteer = require('puppeteer');
const Mustache = require('mustache');

const TEMPLATE_PATH = path.join(__dirname, 'report-template.html');
const REPORTS_DIR = path.join(__dirname, '../reports');

async function generatePDF(reportData) {
  try {
    // Ensure reports directory exists
    await fs.mkdir(REPORTS_DIR, { recursive: true });

    // Load HTML template
    const template = await fs.readFile(TEMPLATE_PATH, 'utf8');

    // Format date
    const date = new Date(reportData.date).toLocaleString('fr-FR', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });

    // Prepare template data
    const templateData = {
      recipeName: reportData.recipeName,
      date: date,
      machineName: reportData.machineName,
      products: reportData.products.map(p => ({
        name: p.name,
        quantity: p.quantity,
        volume: p.volume,
        molar: p.molarMass || p.molar || 0,
      })),
      steps: reportData.steps.map(s => ({
        name: s.name || s.stepName,
        time: s.time || s.duration || 0,
        temp: s.temp || s.temperature || 0,
        pressure: s.pressure || 0,
        speed: s.speed || 0,
        remark: s.remark || s.remarks || (s.warnings ? 'Warning' : 'OK'),
      })),
    };

    // Render HTML with data
    const html = Mustache.render(template, templateData);

    // Launch browser and generate PDF
    const browser = await puppeteer.launch({
      headless: 'new',
      args: ['--no-sandbox', '--disable-setuid-sandbox'],
    });

    const page = await browser.newPage();
    await page.setContent(html, { waitUntil: 'networkidle0' });

    const pdfFilename = `report-${reportData.id || Date.now()}.pdf`;
    const pdfPath = path.join(REPORTS_DIR, pdfFilename);

    await page.pdf({
      path: pdfPath,
      format: 'A4',
      printBackground: true,
      margin: {
        top: '20mm',
        right: '15mm',
        bottom: '20mm',
        left: '15mm',
      },
    });

    await browser.close();

    console.log(`✅ PDF generated: ${pdfFilename}`);
    return pdfPath;
  } catch (error) {
    console.error('❌ Error generating PDF:', error);
    throw error;
  }
}

module.exports = { generatePDF };
