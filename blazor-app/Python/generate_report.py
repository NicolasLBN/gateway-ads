"""
PDF Report Generator for WPF Gateway ADS Application

This script generates PDF reports for completed recipes using ReportLab.
It receives JSON data from the C# WPF application and generates a formatted PDF.
"""

import sys
import json
from datetime import datetime
from reportlab.lib.pagesizes import A4
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.lib.units import cm
from reportlab.platypus import SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle, PageBreak
from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER, TA_LEFT


def generate_pdf_report(data, output_path):
    """
    Generate a PDF report from the provided data
    
    Args:
        data: Dictionary containing report data
        output_path: Path where the PDF should be saved
    """
    # Create the PDF document
    doc = SimpleDocTemplate(output_path, pagesize=A4,
                          rightMargin=2*cm, leftMargin=2*cm,
                          topMargin=2*cm, bottomMargin=2*cm)
    
    # Container for the 'Flowable' objects
    elements = []
    
    # Define styles
    styles = getSampleStyleSheet()
    title_style = ParagraphStyle(
        'CustomTitle',
        parent=styles['Heading1'],
        fontSize=24,
        textColor=colors.HexColor('#1a5490'),
        spaceAfter=30,
        alignment=TA_CENTER
    )
    
    heading_style = ParagraphStyle(
        'CustomHeading',
        parent=styles['Heading2'],
        fontSize=16,
        textColor=colors.HexColor('#2a75bb'),
        spaceAfter=12,
        spaceBefore=12
    )
    
    # Title
    title = Paragraph("Recipe Execution Report", title_style)
    elements.append(title)
    elements.append(Spacer(1, 0.5*cm))
    
    # Report Information
    info_data = [
        ['Recipe Name:', data.get('recipeName', 'N/A')],
        ['Machine:', data.get('machineName', 'N/A')],
        ['Execution Date:', data.get('date', datetime.now().strftime('%Y-%m-%d %H:%M:%S'))],
        ['Report ID:', data.get('id', 'N/A')]
    ]
    
    info_table = Table(info_data, colWidths=[5*cm, 12*cm])
    info_table.setStyle(TableStyle([
        ('FONTNAME', (0, 0), (0, -1), 'Helvetica-Bold'),
        ('FONTNAME', (1, 0), (1, -1), 'Helvetica'),
        ('FONTSIZE', (0, 0), (-1, -1), 11),
        ('TEXTCOLOR', (0, 0), (0, -1), colors.HexColor('#2a75bb')),
        ('ALIGN', (0, 0), (-1, -1), 'LEFT'),
        ('VALIGN', (0, 0), (-1, -1), 'TOP'),
        ('BOTTOMPADDING', (0, 0), (-1, -1), 8),
    ]))
    
    elements.append(info_table)
    elements.append(Spacer(1, 1*cm))
    
    # Ingredients Section
    if 'products' in data and data['products']:
        elements.append(Paragraph("Ingredients", heading_style))
        
        ingredients_data = [['Name', 'Quantity (g)', 'Volume (ml)', 'Molar Mass (g/L)']]
        for product in data['products']:
            ingredients_data.append([
                product.get('name', ''),
                str(product.get('quantity', 0)),
                str(product.get('volume', 0)),
                str(product.get('molarMass', 0))
            ])
        
        ingredients_table = Table(ingredients_data, colWidths=[6*cm, 3.5*cm, 3.5*cm, 4*cm])
        ingredients_table.setStyle(TableStyle([
            ('BACKGROUND', (0, 0), (-1, 0), colors.HexColor('#2a75bb')),
            ('TEXTCOLOR', (0, 0), (-1, 0), colors.whitesmoke),
            ('ALIGN', (0, 0), (-1, -1), 'CENTER'),
            ('FONTNAME', (0, 0), (-1, 0), 'Helvetica-Bold'),
            ('FONTSIZE', (0, 0), (-1, 0), 12),
            ('FONTNAME', (0, 1), (-1, -1), 'Helvetica'),
            ('FONTSIZE', (0, 1), (-1, -1), 10),
            ('BOTTOMPADDING', (0, 0), (-1, 0), 12),
            ('GRID', (0, 0), (-1, -1), 1, colors.grey),
            ('ROWBACKGROUNDS', (0, 1), (-1, -1), [colors.white, colors.HexColor('#f0f0f0')])
        ]))
        
        elements.append(ingredients_table)
        elements.append(Spacer(1, 1*cm))
    
    # Process Steps Section
    if 'steps' in data and data['steps']:
        elements.append(Paragraph("Process Execution Steps", heading_style))
        
        steps_data = [['Step', 'Duration (s)', 'Temp (°C)', 'Pressure (bar)', 'Speed (RPM)', 'Remarks']]
        for step in data['steps']:
            steps_data.append([
                step.get('name', ''),
                str(step.get('time', 0)),
                f"{step.get('temp', 0):.1f}",
                f"{step.get('pressure', 0):.2f}",
                f"{step.get('speed', 0):.0f}",
                step.get('remark', '')
            ])
        
        steps_table = Table(steps_data, colWidths=[4*cm, 2.5*cm, 2.5*cm, 2.5*cm, 2.5*cm, 3*cm])
        steps_table.setStyle(TableStyle([
            ('BACKGROUND', (0, 0), (-1, 0), colors.HexColor('#2a75bb')),
            ('TEXTCOLOR', (0, 0), (-1, 0), colors.whitesmoke),
            ('ALIGN', (0, 0), (-1, -1), 'CENTER'),
            ('FONTNAME', (0, 0), (-1, 0), 'Helvetica-Bold'),
            ('FONTSIZE', (0, 0), (-1, 0), 11),
            ('FONTNAME', (0, 1), (-1, -1), 'Helvetica'),
            ('FONTSIZE', (0, 1), (-1, -1), 9),
            ('BOTTOMPADDING', (0, 0), (-1, 0), 12),
            ('GRID', (0, 0), (-1, -1), 1, colors.grey),
            ('ROWBACKGROUNDS', (0, 1), (-1, -1), [colors.white, colors.HexColor('#f0f0f0')])
        ]))
        
        elements.append(steps_table)
        elements.append(Spacer(1, 1*cm))
    
    # Footer
    footer_style = ParagraphStyle(
        'Footer',
        parent=styles['Normal'],
        fontSize=9,
        textColor=colors.grey,
        alignment=TA_CENTER
    )
    
    elements.append(Spacer(1, 1*cm))
    footer = Paragraph(
        f"Report generated on {datetime.now().strftime('%Y-%m-%d %H:%M:%S')} | Gateway ADS WPF Application",
        footer_style
    )
    elements.append(footer)
    
    # Build PDF
    doc.build(elements)
    print(f"PDF report generated successfully: {output_path}")


def main():
    """Main function to handle command-line execution"""
    if len(sys.argv) < 3:
        print("Usage: python generate_report.py <json_file> <output_pdf>")
        sys.exit(1)
    
    json_file = sys.argv[1]
    output_pdf = sys.argv[2]
    
    try:
        # Read JSON data
        with open(json_file, 'r', encoding='utf-8') as f:
            data = json.load(f)
        
        # Generate PDF
        generate_pdf_report(data, output_pdf)
        
        print("SUCCESS")
        sys.exit(0)
        
    except Exception as e:
        print(f"ERROR: {str(e)}", file=sys.stderr)
        sys.exit(1)


if __name__ == '__main__':
    main()
