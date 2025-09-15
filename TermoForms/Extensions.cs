using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TermoForms;

public static class Extensions
{
    public static void SetComponentBorderRadius(Control component, int radius)
    {
        GraphicsPath path = new GraphicsPath();

        // Canto superior esquerdo
        path.AddArc(0, 0, radius, radius, 180, 90);
        // Linha superior
        path.AddLine(radius, 0, component.Width - radius, 0);
        // Canto superior direito
        path.AddArc(component.Width - radius, 0, radius, radius, 270, 90);
        // Linha direita
        path.AddLine(component.Width, radius, component.Width, component.Height - radius);
        // Canto inferior direito
        path.AddArc(component.Width - radius, component.Height - radius, radius, radius, 0, 90);
        // Linha inferior
        path.AddLine(component.Width - radius, component.Height, radius, component.Height);
        // Canto inferior esquerdo
        path.AddArc(0, component.Height - radius, radius, radius, 90, 90);
        // Linha esquerda
        path.AddLine(0, component.Height - radius, 0, radius);

        path.CloseFigure();
        component.Region = new Region(path);
    }
}
