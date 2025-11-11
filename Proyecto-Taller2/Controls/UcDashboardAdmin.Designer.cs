using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Proyecto_Taller_2.Controls
{
    partial class UcDashboardAdmin
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.FlowLayoutPanel pnlTop;
        private System.Windows.Forms.TableLayoutPanel pnlCenter;
        private System.Windows.Forms.TableLayoutPanel pnlBottom;
        private System.Windows.Forms.Panel mainPanel;
        private Chart salesChart;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        private void InitializeComponent()
        {
            this.pnlTop = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlCenter = new System.Windows.Forms.TableLayoutPanel();
            this.pnlBottom = new System.Windows.Forms.TableLayoutPanel();
            this.mainPanel = new System.Windows.Forms.Panel();
            this.salesChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.mainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.salesChart)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlTop
            // 
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(1315, 150);
            this.pnlTop.TabIndex = 0;
            this.pnlTop.WrapContents = false;
            // 
            // pnlCenter
            // 
            this.pnlCenter.ColumnCount = 2;
            this.pnlCenter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.pnlCenter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.pnlCenter.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlCenter.Location = new System.Drawing.Point(0, 150);
            this.pnlCenter.Name = "pnlCenter";
            this.pnlCenter.Size = new System.Drawing.Size(1315, 344);
            this.pnlCenter.TabIndex = 1;
            // 
            // pnlBottom
            // 
            this.pnlBottom.ColumnCount = 2;
            this.pnlBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.pnlBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBottom.Location = new System.Drawing.Point(0, 494);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(1315, 84);
            this.pnlBottom.TabIndex = 2;
            // 
            // mainPanel
            // 
            this.mainPanel.Controls.Add(this.pnlBottom);
            this.mainPanel.Controls.Add(this.pnlCenter);
            this.mainPanel.Controls.Add(this.pnlTop);
            this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPanel.Location = new System.Drawing.Point(0, 0);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Size = new System.Drawing.Size(1315, 578);
            this.mainPanel.TabIndex = 0;
            // 
            // salesChart
            // 
            this.salesChart.Location = new System.Drawing.Point(0, 0);
            this.salesChart.Name = "salesChart";
            this.salesChart.Size = new System.Drawing.Size(1000, 300);
            this.salesChart.TabIndex = 0;
            // 
            // UcDashboardAdmin
            // 
            this.Controls.Add(this.mainPanel);
            this.Name = "UcDashboardAdmin";
            this.Size = new System.Drawing.Size(1315, 578);
            this.mainPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.salesChart)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
    }
}
