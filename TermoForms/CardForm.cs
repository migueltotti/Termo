using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TermoForms;

public partial class CardForm : Form
{
    public CardForm()
    {
        InitializeComponent();
        SetupCardAppearance();
    }

    private void SetupCardAppearance()
    {
        // Remove bordas do formulário
        this.FormBorderStyle = FormBorderStyle.None;

        // Centraliza na tela pai
        this.StartPosition = FormStartPosition.CenterParent;

        // Cor de fundo
        this.BackColor = Color.White;

        // Adiciona sombra (opcional)
        this.ShowInTaskbar = false;

        Extensions.SetComponentBorderRadius(this, 25);
    }

    private void button1_Click(object sender, EventArgs e)
    {
        this.Dispose();
    }
}
