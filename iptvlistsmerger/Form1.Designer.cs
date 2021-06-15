
namespace iptvlistsmerger
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnMerge = new System.Windows.Forms.Button();
            this.tbSource = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.AllElementsPanel = new System.Windows.Forms.Panel();
            this.TLPAll = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.lblInfo = new System.Windows.Forms.Label();
            this.TPLSourceTargetPaths = new System.Windows.Forms.TableLayoutPanel();
            this.TargetPathPanel = new System.Windows.Forms.Panel();
            this.tbTarget = new System.Windows.Forms.TextBox();
            this.cbxTargets = new System.Windows.Forms.ComboBox();
            this.AllElementsPanel.SuspendLayout();
            this.TLPAll.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.TPLSourceTargetPaths.SuspendLayout();
            this.TargetPathPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnMerge
            // 
            this.btnMerge.Location = new System.Drawing.Point(3, 3);
            this.btnMerge.Name = "btnMerge";
            this.btnMerge.Size = new System.Drawing.Size(115, 29);
            this.btnMerge.TabIndex = 0;
            this.btnMerge.Text = "Merge";
            this.btnMerge.UseVisualStyleBackColor = true;
            this.btnMerge.Click += new System.EventHandler(this.BtnMerge_Click);
            // 
            // tbSource
            // 
            this.tbSource.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tbSource.Location = new System.Drawing.Point(88, 0);
            this.tbSource.Margin = new System.Windows.Forms.Padding(3, 0, 1, 0);
            this.tbSource.Name = "tbSource";
            this.tbSource.Size = new System.Drawing.Size(764, 21);
            this.tbSource.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Source:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 24);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Target:";
            // 
            // AllElementsPanel
            // 
            this.AllElementsPanel.Controls.Add(this.TLPAll);
            this.AllElementsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AllElementsPanel.Location = new System.Drawing.Point(0, 0);
            this.AllElementsPanel.Margin = new System.Windows.Forms.Padding(0);
            this.AllElementsPanel.Name = "AllElementsPanel";
            this.AllElementsPanel.Size = new System.Drawing.Size(853, 85);
            this.AllElementsPanel.TabIndex = 5;
            // 
            // TLPAll
            // 
            this.TLPAll.ColumnCount = 1;
            this.TLPAll.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.TLPAll.Controls.Add(this.tableLayoutPanel3, 0, 1);
            this.TLPAll.Controls.Add(this.TPLSourceTargetPaths, 0, 0);
            this.TLPAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TLPAll.Location = new System.Drawing.Point(0, 0);
            this.TLPAll.Margin = new System.Windows.Forms.Padding(0);
            this.TLPAll.Name = "TLPAll";
            this.TLPAll.RowCount = 2;
            this.TLPAll.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 58.24176F));
            this.TLPAll.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 41.75824F));
            this.TLPAll.Size = new System.Drawing.Size(853, 85);
            this.TLPAll.TabIndex = 0;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel3.Controls.Add(this.btnMerge, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.lblInfo, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 49);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(853, 36);
            this.tableLayoutPanel3.TabIndex = 6;
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.Location = new System.Drawing.Point(173, 0);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(13, 13);
            this.lblInfo.TabIndex = 1;
            this.lblInfo.Text = "_";
            // 
            // TPLSourceTargetPaths
            // 
            this.TPLSourceTargetPaths.ColumnCount = 2;
            this.TPLSourceTargetPaths.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.TPLSourceTargetPaths.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 90F));
            this.TPLSourceTargetPaths.Controls.Add(this.label2, 0, 1);
            this.TPLSourceTargetPaths.Controls.Add(this.label1, 0, 0);
            this.TPLSourceTargetPaths.Controls.Add(this.tbSource, 1, 0);
            this.TPLSourceTargetPaths.Controls.Add(this.TargetPathPanel, 1, 1);
            this.TPLSourceTargetPaths.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TPLSourceTargetPaths.Location = new System.Drawing.Point(0, 0);
            this.TPLSourceTargetPaths.Margin = new System.Windows.Forms.Padding(0);
            this.TPLSourceTargetPaths.Name = "TPLSourceTargetPaths";
            this.TPLSourceTargetPaths.RowCount = 2;
            this.TPLSourceTargetPaths.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TPLSourceTargetPaths.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.TPLSourceTargetPaths.Size = new System.Drawing.Size(853, 49);
            this.TPLSourceTargetPaths.TabIndex = 0;
            // 
            // TargetPathPanel
            // 
            this.TargetPathPanel.Controls.Add(this.tbTarget);
            this.TargetPathPanel.Controls.Add(this.cbxTargets);
            this.TargetPathPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TargetPathPanel.Location = new System.Drawing.Point(88, 27);
            this.TargetPathPanel.Name = "TargetPathPanel";
            this.TargetPathPanel.Size = new System.Drawing.Size(762, 19);
            this.TargetPathPanel.TabIndex = 5;
            // 
            // tbTarget
            // 
            this.tbTarget.Location = new System.Drawing.Point(0, 0);
            this.tbTarget.Name = "tbTarget";
            this.tbTarget.Size = new System.Drawing.Size(745, 21);
            this.tbTarget.TabIndex = 1;
            // 
            // cbxTargets
            // 
            this.cbxTargets.Dock = System.Windows.Forms.DockStyle.Top;
            this.cbxTargets.FormattingEnabled = true;
            this.cbxTargets.Location = new System.Drawing.Point(0, 0);
            this.cbxTargets.Name = "cbxTargets";
            this.cbxTargets.Size = new System.Drawing.Size(762, 21);
            this.cbxTargets.TabIndex = 0;
            this.cbxTargets.SelectedIndexChanged += new System.EventHandler(this.cbxTargets_SelectedIndexChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(853, 85);
            this.Controls.Add(this.AllElementsPanel);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.AllElementsPanel.ResumeLayout(false);
            this.TLPAll.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.TPLSourceTargetPaths.ResumeLayout(false);
            this.TPLSourceTargetPaths.PerformLayout();
            this.TargetPathPanel.ResumeLayout(false);
            this.TargetPathPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnMerge;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel AllElementsPanel;
        private System.Windows.Forms.TableLayoutPanel TLPAll;
        internal System.Windows.Forms.TextBox tbSource;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        internal System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.TableLayoutPanel TPLSourceTargetPaths;
        private System.Windows.Forms.Panel TargetPathPanel;
        private System.Windows.Forms.TextBox tbTarget;
        private System.Windows.Forms.ComboBox cbxTargets;
    }
}

