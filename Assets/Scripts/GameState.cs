using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum GameState
{
    Demo,       // Демонстрационный режим
    Loading,    // Экран загрузки
    Game,       // Основной режим
    Crushed,    // Игрок уничтожен
    GameOver    // Игрок проиграл
}
