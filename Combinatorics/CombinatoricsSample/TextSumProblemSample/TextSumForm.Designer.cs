namespace TextSumProblemSample {
    partial class TextSumForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextSumForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.problemSumLabel = new System.Windows.Forms.Label();
            this.problemOperand2Label = new System.Windows.Forms.Label();
            this.problemOperand1Label = new System.Windows.Forms.Label();
            this.operand1Text = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.operand2Text = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.sumText = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.solutionSumLabel = new System.Windows.Forms.Label();
            this.solutionOperand2Label = new System.Windows.Forms.Label();
            this.solutionOperand1Label = new System.Windows.Forms.Label();
            this.solutionNumberComboBox = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusStripLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.problemSumLabel);
            this.groupBox1.Controls.Add(this.problemOperand2Label);
            this.groupBox1.Controls.Add(this.problemOperand1Label);
            this.groupBox1.Location = new System.Drawing.Point(164, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(126, 127);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Problem:";
            this.groupBox1.UseCompatibleTextRendering = true;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(16, 75);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(11, 16);
            this.label1.TabIndex = 6;
            this.label1.Text = "+";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // problemSumLabel
            // 
            this.problemSumLabel.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.problemSumLabel.Location = new System.Drawing.Point(12, 91);
            this.problemSumLabel.Name = "problemSumLabel";
            this.problemSumLabel.Size = new System.Drawing.Size(100, 16);
            this.problemSumLabel.TabIndex = 5;
            this.problemSumLabel.Text = "R I G H T";
            this.problemSumLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // problemOperand2Label
            // 
            this.problemOperand2Label.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.problemOperand2Label.Location = new System.Drawing.Point(12, 75);
            this.problemOperand2Label.Name = "problemOperand2Label";
            this.problemOperand2Label.Size = new System.Drawing.Size(100, 16);
            this.problemOperand2Label.TabIndex = 4;
            this.problemOperand2Label.Text = " W R O N G";
            this.problemOperand2Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // problemOperand1Label
            // 
            this.problemOperand1Label.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.problemOperand1Label.Location = new System.Drawing.Point(12, 59);
            this.problemOperand1Label.Name = "problemOperand1Label";
            this.problemOperand1Label.Size = new System.Drawing.Size(100, 16);
            this.problemOperand1Label.TabIndex = 3;
            this.problemOperand1Label.Text = "W R O N G";
            this.problemOperand1Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // operand1Text
            // 
            this.operand1Text.Location = new System.Drawing.Point(80, 19);
            this.operand1Text.MaxLength = 5;
            this.operand1Text.Name = "operand1Text";
            this.operand1Text.Size = new System.Drawing.Size(67, 20);
            this.operand1Text.TabIndex = 1;
            this.operand1Text.Text = "WRONG";
            this.operand1Text.TextChanged += new System.EventHandler(this.op1Text_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(60, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Operand 1:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(60, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Operand 2:";
            // 
            // operand2Text
            // 
            this.operand2Text.Location = new System.Drawing.Point(80, 45);
            this.operand2Text.MaxLength = 5;
            this.operand2Text.Name = "operand2Text";
            this.operand2Text.Size = new System.Drawing.Size(67, 20);
            this.operand2Text.TabIndex = 2;
            this.operand2Text.Text = "WRONG";
            this.operand2Text.TextChanged += new System.EventHandler(this.op2Text_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(14, 74);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(31, 13);
            this.label6.TabIndex = 9;
            this.label6.Text = "Sum:";
            // 
            // sumText
            // 
            this.sumText.Location = new System.Drawing.Point(80, 71);
            this.sumText.MaxLength = 5;
            this.sumText.Name = "sumText";
            this.sumText.Size = new System.Drawing.Size(67, 20);
            this.sumText.TabIndex = 3;
            this.sumText.Text = "RIGHT";
            this.sumText.TextChanged += new System.EventHandler(this.sumText_TextChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.solutionSumLabel);
            this.groupBox2.Controls.Add(this.solutionOperand2Label);
            this.groupBox2.Controls.Add(this.solutionOperand1Label);
            this.groupBox2.Controls.Add(this.solutionNumberComboBox);
            this.groupBox2.Location = new System.Drawing.Point(303, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(126, 127);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Solution(s):";
            this.groupBox2.UseCompatibleTextRendering = true;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(17, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(11, 16);
            this.label2.TabIndex = 15;
            this.label2.Text = "+";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // solutionSumLabel
            // 
            this.solutionSumLabel.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.solutionSumLabel.Location = new System.Drawing.Point(13, 91);
            this.solutionSumLabel.Name = "solutionSumLabel";
            this.solutionSumLabel.Size = new System.Drawing.Size(100, 16);
            this.solutionSumLabel.TabIndex = 14;
            this.solutionSumLabel.Text = "0 0 0 0 0";
            this.solutionSumLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // solutionOperand2Label
            // 
            this.solutionOperand2Label.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.solutionOperand2Label.Location = new System.Drawing.Point(13, 75);
            this.solutionOperand2Label.Name = "solutionOperand2Label";
            this.solutionOperand2Label.Size = new System.Drawing.Size(100, 16);
            this.solutionOperand2Label.TabIndex = 13;
            this.solutionOperand2Label.Text = " 0 0 0 0 0";
            this.solutionOperand2Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // solutionOperand1Label
            // 
            this.solutionOperand1Label.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.solutionOperand1Label.Location = new System.Drawing.Point(13, 59);
            this.solutionOperand1Label.Name = "solutionOperand1Label";
            this.solutionOperand1Label.Size = new System.Drawing.Size(100, 16);
            this.solutionOperand1Label.TabIndex = 12;
            this.solutionOperand1Label.Text = "0 0 0 0 0";
            this.solutionOperand1Label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // solutionNumberComboBox
            // 
            this.solutionNumberComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.solutionNumberComboBox.FormattingEnabled = true;
            this.solutionNumberComboBox.Location = new System.Drawing.Point(36, 22);
            this.solutionNumberComboBox.Name = "solutionNumberComboBox";
            this.solutionNumberComboBox.Size = new System.Drawing.Size(77, 21);
            this.solutionNumberComboBox.TabIndex = 11;
            this.solutionNumberComboBox.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(80, 116);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(67, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "&Solve";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusStripLabel,
            this.statusStripProgressBar});
            this.statusStrip.Location = new System.Drawing.Point(0, 151);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(449, 22);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 13;
            this.statusStrip.Text = "statusStrip1";
            // 
            // statusStripLabel
            // 
            this.statusStripLabel.Name = "statusStripLabel";
            this.statusStripLabel.Size = new System.Drawing.Size(332, 17);
            this.statusStripLabel.Spring = true;
            this.statusStripLabel.Text = "Ready";
            this.statusStripLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statusStripProgressBar
            // 
            this.statusStripProgressBar.Name = "statusStripProgressBar";
            this.statusStripProgressBar.Size = new System.Drawing.Size(100, 16);
            // 
            // TextSumForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(449, 173);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.sumText);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.operand2Text);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.operand1Text);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "TextSumForm";
            this.Text = "Combinatorics Text Sum Problem Sample";
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label problemSumLabel;
        private System.Windows.Forms.Label problemOperand2Label;
        private System.Windows.Forms.Label problemOperand1Label;
        private System.Windows.Forms.TextBox operand1Text;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox operand2Text;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox sumText;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.ComboBox solutionNumberComboBox;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label solutionSumLabel;
        private System.Windows.Forms.Label solutionOperand2Label;
        private System.Windows.Forms.Label solutionOperand1Label;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusStripLabel;
        private System.Windows.Forms.ToolStripProgressBar statusStripProgressBar;
    }
}

