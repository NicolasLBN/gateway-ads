const fs = require('fs').promises;
const path = require('path');
const Mustache = require('mustache');

const TEMPLATE_PATH = path.join(__dirname, 'pdf', 'report-template.html');
const REPORTS_DIR = path.join(__dirname, 'reports');
const HISTORY_FILE = path.join(__dirname, 'data', 'history.json');

async function generateSampleReports() {
  try {
    console.log('Loading history data...');
    const data = await fs.readFile(HISTORY_FILE, 'utf8');
    const history = JSON.parse(data);

    console.log(`Found ${history.length} reports to generate...`);

    // Load HTML template
    const template = await fs.readFile(TEMPLATE_PATH, 'utf8');

    for (const report of history) {
      console.log(`\nGenerating report for: ${report.recipeName} (ID: ${report.id})`);
      
      // Format date
      const date = new Date(report.date).toLocaleString('fr-FR', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
      });

      // Prepare template data
      const templateData = {
        recipeName: report.recipeName,
        date: date,
        machineName: report.machineName,
        products: report.products.map(p => ({
          name: p.name,
          quantity: p.quantity,
          volume: p.volume,
          molar: p.molar || 0,
        })),
        steps: report.steps.map(s => ({
          name: s.name,
          time: s.time || 0,
          temp: s.temp || 0,
          pressure: s.pressure || 0,
          speed: s.speed || 0,
          remark: s.remark || 'ok',
        })),
      };

      // Render HTML with data
      const html = Mustache.render(template, templateData);
      
      // Save as HTML file
      const htmlFilename = `report_${report.id}.html`;
      const htmlPath = path.join(REPORTS_DIR, htmlFilename);
      
      await fs.writeFile(htmlPath, html, 'utf8');
      console.log(`✅ Report created: ${htmlFilename}`);
    }

    console.log('\n✅ All sample reports generated successfully!');
  } catch (error) {
    console.error('❌ Error generating sample reports:', error);
    process.exit(1);
  }
}

generateSampleReports();
