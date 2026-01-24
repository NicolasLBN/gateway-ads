const { generatePDF } = require('./pdf/pdfGenerator');
const fs = require('fs').promises;
const path = require('path');

// Load history data
const HISTORY_FILE = path.join(__dirname, 'data/history.json');

async function generateSamplePDFs() {
  try {
    console.log('Loading history data...');
    const data = await fs.readFile(HISTORY_FILE, 'utf8');
    const history = JSON.parse(data);

    console.log(`Found ${history.length} reports to generate PDFs for...`);

    for (const report of history) {
      console.log(`\nGenerating PDF for: ${report.recipeName} (ID: ${report.id})`);
      
      // Generate the PDF with the exact filename expected
      const pdfFilename = `report_${report.id}.pdf`;
      const pdfPath = path.join(__dirname, 'reports', pdfFilename);
      
      // Create a modified report with the correct ID format for filename
      const reportForPDF = {
        ...report,
        id: report.id, // Use the ID as is
      };
      
      await generatePDF(reportForPDF);
      
      // Rename the file to match expected format (report_1.pdf instead of report-1.pdf)
      const generatedPath = path.join(__dirname, 'reports', `report-${report.id}.pdf`);
      try {
        await fs.rename(generatedPath, pdfPath);
        console.log(`✅ PDF created: ${pdfFilename}`);
      } catch (err) {
        console.log(`File already has correct name: ${pdfFilename}`);
      }
    }

    console.log('\n✅ All sample PDFs generated successfully!');
  } catch (error) {
    console.error('❌ Error generating sample PDFs:', error);
    process.exit(1);
  }
}

generateSamplePDFs();
