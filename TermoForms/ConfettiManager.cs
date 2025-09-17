using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

// Classe separada para gerenciar os confetes
public class ConfettiManager
{
    private System.Windows.Forms.Timer confettiTimer;
    private List<Confetti> confettiList;
    private Random random;
    private Control targetControl; // O controle onde os confetes serão desenhados

    private readonly Color[] confettiColors = {
        Color.Red, Color.Blue, Color.Green, Color.Yellow,
        Color.Orange, Color.Purple, Color.Pink, Color.Cyan,
        Color.Magenta, Color.Lime
    };

    public ConfettiManager(Control control)
    {
        targetControl = control;
        Initialize();
    }

    // Classe para representar um confete individual
    public class Confetti
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float VelocityY { get; set; }
        public float VelocityX { get; set; }
        public Color Color { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public float Rotation { get; set; }
        public float RotationSpeed { get; set; }

        public Confetti(float x, float y, Color color, Random random)
        {
            X = x;
            Y = y;
            Color = color;
            Width = 8;
            Height = 12;
            VelocityY = (float)(random.NextDouble() * 3 + 2); // Velocidade vertical entre 2-5
            VelocityX = (float)(random.NextDouble() * 2 - 1); // Velocidade horizontal entre -1 e 1
            Rotation = 0;
            RotationSpeed = (float)(random.NextDouble() * 10 - 5); // Rotação entre -5 e 5
        }

        public void Update()
        {
            Y += VelocityY;
            X += VelocityX;
            Rotation += RotationSpeed;

            // Adiciona um pouco de oscilação horizontal
            VelocityX += (float)(Math.Sin(Y * 0.01) * 0.1);
        }
    }

    private void Initialize()
    {
        confettiList = new List<Confetti>();
        random = new Random();

        // Configurar o timer
        confettiTimer = new System.Windows.Forms.Timer();
        confettiTimer.Interval = 30; // ~33 FPS
        confettiTimer.Tick += ConfettiTimer_Tick;
    }

    // Método principal para iniciar a animação de confetes
    public void StartAnimation(int totalConfetti = 50)
    {
        confettiList.Clear();

        // Criar todos os confetes de uma vez
        for (int i = 0; i < totalConfetti; i++)
        {
            // Espalhar os confetes ao longo da largura da tela
            float x = random.Next(-50, targetControl.Width + 50);
            // Posicionar em diferentes alturas para não caírem todos juntos
            float y = random.Next(-200, -20);
            Color color = confettiColors[random.Next(confettiColors.Length)];
            confettiList.Add(new Confetti(x, y, color, random));
        }

        confettiTimer.Start();
    }

    public void StopAnimation()
    {
        confettiTimer.Stop();
        confettiList.Clear();
    }

    public bool IsAnimating => confettiTimer.Enabled && confettiList.Count > 0;

    private void ConfettiTimer_Tick(object sender, EventArgs e)
    {
        // Atualizar posição dos confetes
        for (int i = confettiList.Count - 1; i >= 0; i--)
        {
            confettiList[i].Update();

            // Remover confetes que saíram da tela
            if (confettiList[i].Y > targetControl.Height + 50 ||
                confettiList[i].X < -100 ||
                confettiList[i].X > targetControl.Width + 100)
            {
                confettiList.RemoveAt(i);
            }
        }

        // Parar o timer quando não houver mais confetes na tela
        if (confettiList.Count == 0)
        {
            confettiTimer.Stop();
        }

        // Redesenhar o controle
        targetControl.Invalidate();
    }

    // Método para criar burst de confetes em posição específica
    public void CreateBurst(int x, int y, int count = 20)
    {
        for (int i = 0; i < count; i++)
        {
            float offsetX = (float)(random.NextDouble() * 200 - 100); // Espalhamento horizontal
            Color color = confettiColors[random.Next(confettiColors.Length)];
            confettiList.Add(new Confetti(x + offsetX, y, color, random));
        }

        if (!confettiTimer.Enabled)
        {
            confettiTimer.Start();
        }
    }

    // Método para desenhar os confetes - deve ser chamado no Paint do controle
    public void Draw(Graphics graphics)
    {
        foreach (var confetti in confettiList)
        {
            DrawConfetti(graphics, confetti);
        }
    }

    private void DrawConfetti(Graphics graphics, Confetti confetti)
    {
        // Salvar o estado atual da transformação
        var state = graphics.Save();

        try
        {
            // Mover para a posição do confete e rotacionar
            graphics.TranslateTransform(confetti.X, confetti.Y);
            graphics.RotateTransform(confetti.Rotation);

            // Desenhar o retângulo do confete
            using (Brush brush = new SolidBrush(confetti.Color))
            {
                graphics.FillRectangle(brush,
                    -confetti.Width / 2,
                    -confetti.Height / 2,
                    confetti.Width,
                    confetti.Height);
            }

            // Adicionar uma borda mais escura para dar profundidade
            using (Pen pen = new Pen(Color.FromArgb(100, Color.Black), 1))
            {
                graphics.DrawRectangle(pen,
                    -confetti.Width / 2,
                    -confetti.Height / 2,
                    confetti.Width,
                    confetti.Height);
            }
        }
        finally
        {
            // Restaurar o estado da transformação
            graphics.Restore(state);
        }
    }

    // Limpar recursos
    public void Dispose()
    {
        confettiTimer?.Stop();
        confettiTimer?.Dispose();
        confettiList?.Clear();
    }
}