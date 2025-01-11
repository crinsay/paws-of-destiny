using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PawsOfDestiny.Scripts.Constants;

internal enum MoveDirection
{
    /// <summary>
    /// <para>No movement or direction. Used for actions without assigned keys.</para>
    /// </summary>
    None = -1,
    /// <summary>
    /// <para>Moves the player to the left. It is a constant movement.</para>
    /// </summary>
    MoveLeft = 0,
    /// <summary>
    /// <para>Moves the player to the right. It is a constant movement.</para>
    /// </summary>
    MoveRight = 1,
    /// <summary>
    /// <para>Moves the player up. <b>It is NOT a constant movement</b> and should be used only for actions like jumping etc..</para>
    /// </summary>
    MoveUp = 2,
    /// <summary>
    /// <para>Moves the player down. <b>It is NOT a constant movement</b> and should be used only for actions like going down through platforms etc..</para>
    /// </summary>
    MoveDown = 3
}
