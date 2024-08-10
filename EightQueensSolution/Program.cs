using System.Drawing;

const int attacked = 1;

var result = string.Join('\n', Solve(CreateBoard()).Select(points => string.Join(", ", points)));
Console.WriteLine(result.Length > 0 ? $"Solutions found: \n{result}" : "No solutions found :(");

return;

static int[,] CreateBoard(int size = 8)
{
    return new int[size, size];
}

static int[,] CloneBoard(int[,] board)
{
    var clone = new int[board.GetLength(0),board.GetLength(1)];

    foreach (var point in GetPoints(board))
    {
        clone[point.Y, point.X] = board[point.Y, point.X];
    }

    return clone;
}

static void PlaceQueen(int[,] board, Point point)
{
    foreach (var attackingPoint in GetAttackingPoints(board, point))
    {
        board[attackingPoint.Y, attackingPoint.X] = attacked;
    }
}

static IEnumerable<Point> GetPoints(int[,] board)
{
    for (int y = 0; y < board.GetLength(0); y++)
    {
        for (int x = 0; x < board.GetLength(1); x++)
        {
            yield return new Point(x, y);
        }
    }
}

static IEnumerable<Point> GetVerticalPoints(int[,] board, int x)
{
    if (x < 0 || x >= board.GetLength(1))
    {
        yield break;
    }
    
    for (int y = 0; y < board.GetLength(0); y++)
    {
        yield return new Point(x, y);
    }
}

static IEnumerable<Point> GetHorizontalPoints(int[,] board, int y)
{
    if (y < 0 || y >= board.GetLength(0))
    {
        yield break;
    }
    
    for (int x = 0; x < board.GetLength(1); x++)
    {
        yield return new Point(x, y);
    }
}

static IEnumerable<Point> GetLeadingDiagonalPoints(int[,] board, Point point)
{
    var lowest = int.Min(point.X, point.Y);
    var diagonalPoint = new Point(point.X - lowest, point.Y - lowest);

    for (; diagonalPoint.X < board.GetLength(1) && diagonalPoint.Y < board.GetLength(0); diagonalPoint.X++, diagonalPoint.Y++)
    {
        yield return diagonalPoint;
    }
}

static IEnumerable<Point> GetAntiDiagonalPoints(int[,] board, Point point)
{
    var xDistanceFromEdge = board.GetLength(1) - 1 - point.X;
    var lowest = int.Min(xDistanceFromEdge, point.Y);
    var diagonalPoint = new Point(point.X + lowest, point.Y - lowest);

    for (; diagonalPoint.X >= 0 && diagonalPoint.Y < board.GetLength(0); diagonalPoint.X--, diagonalPoint.Y++)
    {
        yield return diagonalPoint;
    }
}

static IEnumerable<Point> GetAttackingPoints(int[,] board, Point point)
{
    foreach (var item in GetVerticalPoints(board, point.X)
        .Concat(GetHorizontalPoints(board, point.Y))
        .Concat(GetLeadingDiagonalPoints(board, point))
        .Concat(GetAntiDiagonalPoints(board, point)))
    {
        yield return item;
    }
}

static List<Point[]> Solve(int[,] board)
{
    return SolveRecursively(board, 0, []);

    List<Point[]> SolveRecursively(int[,] board, int col, IEnumerable<Point> placedQueens)
    {
        var results = new List<Point[]>();

        foreach (var point in GetLowestAttackCountPoints(board, col))
        {
            var newBoard = CloneBoard(board);
            PlaceQueen(newBoard, point);
            
            var result = SolveRecursively(newBoard, col + 1, placedQueens.Append(point));
            results.AddRange(result);
        }

        if (col == board.GetLength(0))
        {
            results.Add(placedQueens.ToArray());
        }

        return results;
    }
}

static IEnumerable<Point> GetLowestAttackCountPoints(int[,] board, int col)
{
    return GetVerticalPoints(board, col)
        .Aggregate((Result: new List<Point>(), LowestAttackCount: int.MaxValue), (result, point) =>
        {
            var (lowestAttackCountPoints, lowestAttackCount) = result;
            var attackCount = GetAttackCount(board, point);

            if (attackCount == -1)
            {
                return result;
            }

            if (attackCount == lowestAttackCount)
            {
                lowestAttackCountPoints.Add(point);
            }
            else if (attackCount < lowestAttackCount)
            {
                lowestAttackCount = attackCount;
                lowestAttackCountPoints.Clear();
                lowestAttackCountPoints.Add(point);
            }

            return (lowestAttackCountPoints, lowestAttackCount);
        }).Result;
}

static int GetAttackCount(int[,] board, Point point)
{
    if (IsOffBoardPoint() || IsEliminatedSquare())
    {
        return -1;
    }
    
    const int duplicateSquareCount = 3;

    return GetAttackingPoints(board, point)
        .Select(point => board[point.Y, point.X])
        .Count(square => square is not attacked) - duplicateSquareCount;

    bool IsOffBoardPoint() => point.Y < 0 ||
                              point.Y >= board.GetLength(0) && 
                              point.X < 0 || 
                              point.X >= board.GetLength(1);
    bool IsEliminatedSquare() => board[point.Y, point.X] is attacked;
}