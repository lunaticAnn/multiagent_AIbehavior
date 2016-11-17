using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TextSumProblemSample {
    /// <summary>
    /// UI wrapper around the text problem, not much interesting is happening here.
    /// The problem solving is completely delegated to the TextSumProblem class.
    /// </summary>
    public partial class TextSumForm : Form {
        public TextSumForm() {
            InitializeComponent();
            op1Text_TextChanged(null, null);
            op2Text_TextChanged(null, null);
            sumText_TextChanged(null, null);
        }

        private void op1Text_TextChanged(object sender, EventArgs e) {
            op1.Clear();
            for(int i = 0; i < operand1Text.Text.Length; ++i) {
                op1.Add(operand1Text.Text.Substring(i, 1));
            }
            UpdateProblem();
        }

        private void op2Text_TextChanged(object sender, EventArgs e) {
            op2.Clear();
            for(int i = 0; i < operand2Text.Text.Length; ++i) {
                op2.Add(operand2Text.Text.Substring(i, 1));
            }
            UpdateProblem();
        }

        private void sumText_TextChanged(object sender, EventArgs e) {
            sum.Clear();
            for(int i = 0; i < sumText.Text.Length; ++i) {
                sum.Add(sumText.Text.Substring(i, 1));
            }
            UpdateProblem();
        }

        private void UpdateProblem() {
            problemOperand1Label.Text = String.Join(" ", op1.ToArray());
            problemOperand2Label.Text = String.Join(" ", op2.ToArray());
            problemOperand2Label.Text = "          ".Substring(0, 10 - problemOperand2Label.Text.Length) + problemOperand2Label.Text;
            problemSumLabel.Text = String.Join(" ", sum.ToArray());
            solutionNumberComboBox.Items.Clear();
            statusStripLabel.Text = "Ready";
        }

        private List<string> op1 = new List<string>();
        private List<string> op2 = new List<string>();
        private List<string> sum = new List<string>();

        private void button1_Click(object sender, EventArgs e) {
            button1.Enabled = false;
            ClearSolutions();
            TextSumProblem problem = new TextSumProblem();
            problem.OnProgress += new TextSumProblem.ProgressDelegate(problem_OnProgress);
            problem.OnSolution += new TextSumProblem.AnswerDelegate(problem_OnSolution);
            Application.DoEvents();
            problem.Solve(operand1Text.Text, operand2Text.Text, sumText.Text);
            statusStripLabel.Text += " Done.";
            button1.Enabled = true;
        }

        private void ClearSolutions() {
            solutions.Clear();
            solutionNumberComboBox.Items.Clear();
            solutionNumberComboBox.Text = "";
            string allBlanks = " 0 0 0 0 0";
            solutionOperand1Label.Text = allBlanks;
            solutionOperand2Label.Text = allBlanks;
            solutionSumLabel.Text = allBlanks;
        }

        private void problem_OnProgress(object sender, int current, int total) {
            if(current == 0) {
                statusStripLabel.Text = String.Format("Searching {0} Variations...", total);
                Application.DoEvents();
            }
            // Only update periodically to prevent UI CPU drain.
            if(current == total || current % 181 == 0) {
                statusStripProgressBar.Minimum = 0;
                statusStripProgressBar.Maximum = total;
                statusStripProgressBar.Value = current;
                Application.DoEvents();
            }
        }

        private void problem_OnSolution(object sender, Dictionary<char, int> solution) {
            solutions.Add(solution);
            DisplaySolution(solutions.Count - 1);
            solutionNumberComboBox.Items.Add(solutions.Count);
            solutionNumberComboBox.SelectedIndex = solutions.Count - 1;
            Application.DoEvents();
        }

        private void DisplaySolution(int index) {
            Dictionary<char, int> solution = solutions[index];
            solutionOperand1Label.Text = problemOperand1Label.Text;
            solutionOperand2Label.Text = problemOperand2Label.Text;
            solutionSumLabel.Text = problemSumLabel.Text;
            foreach(KeyValuePair<char, int> pair in solution) {
                solutionOperand1Label.Text = solutionOperand1Label.Text.Replace(pair.Key.ToString(), pair.Value.ToString());
                solutionOperand2Label.Text = solutionOperand2Label.Text.Replace(pair.Key.ToString(), pair.Value.ToString());
                solutionSumLabel.Text = solutionSumLabel.Text.Replace(pair.Key.ToString(), pair.Value.ToString());
            }
        }

        private List<Dictionary<char, int>> solutions = new List<Dictionary<char, int>>();

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) {
            DisplaySolution(solutionNumberComboBox.SelectedIndex);
        }
    }
}