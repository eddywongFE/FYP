using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Evaluator
{
    private char[] board;
    private double value;

    public Evaluator(char[] board, double value) {
        this.board = board;
        this.value = value;
    }

    public void setBoard(char[] board) { this.board = board; }
    public void setValue(double value) { this.value = value; }

    public char[] getBoard() { return this.board; }
    public double getValue() { return this.value; }
}

public class AIBot
{
    private AIBot(){}
    private static AIBot instance = null;
    private GameController gameController;
    private int removeTokenIndex = -1;

    // Get Instance to the AIBot
    public static AIBot getInstance() {
        if (AIBot.instance == null) {
            AIBot.instance = new AIBot();
        }

        return AIBot.instance;
    }

    // Set the GameController reference
    public void SetGameControllerReferences(GameController controller) {
        gameController = controller;
    }

    // Select the next move to place the token
    public void SelectPlacingSpot(char[] board, int phase, int token, char side, bool difficulty) {
        int depth = (difficulty) ? 3 : 1;
        Evaluator result = alphaBetaPruning(board, depth, double.NegativeInfinity, double.PositiveInfinity, true, token, phase, 0, difficulty);
        int destination = GetPlacedPieces(board, result.getBoard());

        if (CheckFormedMill(result.getBoard(), destination, side)) {
            int removed = GetMovedPieces(board, result.getBoard(), gameController.getOppositeSide(side));
            SetRemovingToken(removed);
        }

        if (phase == 1) {
            gameController.spotList[destination].GetComponent<Spot>().SetToken();
        } else {
            int from = GetMovedPieces(board, result.getBoard(), side);
            gameController.SetCurrentToken(from);
            gameController.spotList[destination].GetComponent<Spot>().SetToken();
        }
    }

    // Alpha beta pruning algorithm
    private Evaluator alphaBetaPruning(char[] board, int depth, double alpha, double beta, bool maximize, int token, int phase, double score, bool difficulty) {
        if (depth == 0 || CheckTerminated(board, phase) != 'N') {
            return new Evaluator(null, score);
        }

        if (phase == 1 && token >= 18) {
            phase = 2;
        } else if (phase == 2 && (CountPieces(board, 'W') <= 3 || CountPieces(board, 'B') <= 3)) {
            phase = 3;
        }

        if (maximize) {
            char[] bestResult = null;
            double bestScore = double.NegativeInfinity;

            ArrayList possibleState = GetNextState(board, phase, 'B');
            foreach (char[] state in possibleState) {
                int index = GetPlacedPieces(board, state);
                double evaluation = StateEvaluation(state, phase, index, 'B', score, difficulty);
                Evaluator result = alphaBetaPruning(state, depth - 1, alpha, beta, false, token + 1, phase, evaluation, difficulty);
                double currentScore = result.getValue();

                if (currentScore > bestScore) {
                    bestScore = currentScore;
                    bestResult = state;
                }

                if (bestScore > alpha) {
                    alpha = bestScore;
                }

                if (alpha >= beta) {
                    break;
                }
            }

            return new Evaluator(bestResult, bestScore);
        } else {
            char[] bestResult = null;
            double bestScore = double.PositiveInfinity;

            ArrayList possibleState = GetNextState(board, phase, 'W');
            foreach (char[] state in possibleState) {
                int index = GetPlacedPieces(board, state);
                double evaluation = StateEvaluation(state, phase, index, 'W', score, difficulty);
                Evaluator result = alphaBetaPruning(state, depth - 1, alpha, beta, true, token + 1, phase, evaluation, difficulty);
                double currentScore = result.getValue();

                if (currentScore < bestScore) {
                    bestScore = currentScore;
                    bestResult = state;
                }

                if (bestScore < beta) {
                    beta = bestScore;
                }

                if (alpha >= beta) {
                    break;
                }
            }

            return new Evaluator(bestResult, bestScore);
        }
    }

    // Get the moved pieces index
    private int GetMovedPieces(char[] prev, char[] current, char side) {
        int index = -1;
        for (int i = 0; i < prev.Length; i++) {
            if (prev[i] == side && current[i] == 'N') {
                index = i;
            }
        }

        return index;
    }

    // Get next possible state
    private ArrayList GetNextState(char[] board, int phase, char side) {
        ArrayList possibleMove = new ArrayList();

        if (phase == 1) {
            for (int i = 0; i < board.Length; i++) {
                if (board[i] == 'N') {
                    char[] nextBoard = (char[])board.Clone();
                    nextBoard[i] = side;
                    if (CheckFormedMill(nextBoard, i, side)) {
                        ArrayList moreResult = GetRemoveOpponentResult(nextBoard, side);
                        foreach (char[] result in moreResult) {
                            possibleMove.Add(result);
                        }
                    } else {
                        possibleMove.Add(nextBoard);
                    }
                }
            }
        } else if (phase == 2) {
            for (int i = 0; i < board.Length; i++) {
                if (board[i] == side) {
                    int[] adjacent = gameController.GetAdjacentNode(i);
                    for (int x = 0; x < adjacent.Length; x++) {
                        int index = adjacent[x];

                        if (board[index] == 'N') {
                            char[] nextBoard = (char[])board.Clone();
                            nextBoard[i] = 'N';
                            nextBoard[index] = side;
                            if (CheckFormedMill(nextBoard, index, side)) {
                                ArrayList moreResult = GetRemoveOpponentResult(nextBoard, side);
                                foreach (char[] result in moreResult) {
                                    possibleMove.Add(result);
                                }
                            } else {
                                possibleMove.Add(nextBoard);
                            }
                        }
                    }
                }
            }
        } else {
            int count = CountPieces(board, side);

            for (int i = 0; i < board.Length; i++) {
                if (board[i] == side) {
                    int[] adjacent = (count <= 3) ? Enumerable.Range(0, 24).ToArray() : gameController.GetAdjacentNode(i);
                    for (int x = 0; x < adjacent.Length; x++) {
                        int index = adjacent[x];

                        if (board[index] == 'N') {
                            char[] nextBoard = (char[])board.Clone();
                            nextBoard[i] = 'N';
                            nextBoard[index] = side;
                            if (CheckFormedMill(nextBoard, index, side)) {
                                ArrayList moreResult = GetRemoveOpponentResult(nextBoard, side);
                                foreach (char[] result in moreResult) {
                                    possibleMove.Add(result);
                                }
                            } else {
                                possibleMove.Add(nextBoard);
                            }
                        }
                    }
                }
            }   
        }

        return possibleMove;
    }

    // Get possible result of removed opponent piece
    private ArrayList GetRemoveOpponentResult(char[] board, char side) {
        ArrayList possibleResult = new ArrayList();
        bool all = AllInMill(board, gameController.getOppositeSide(side));

        for (int i = 0; i < board.Length; i++) {
            if (board[i] == gameController.getOppositeSide(side)) {
                if (!all && CheckFormedMill(board, i, gameController.getOppositeSide(side))) {
                    continue;
                } else {
                    char[] result = (char[])board.Clone();
                    result[i] = 'N';
                    possibleResult.Add(result);
                }
            }
        }
        return possibleResult;
    }

    // Check if all opponent pieces in mill
    private bool AllInMill(char[] board, char side) {
        int pieces = 0, inMill = 0;

        for (int i = 0; i < board.Length; i++) {
            if (board[i] == side) {
                pieces++;
                if (CheckFormedMill(board, i, side) == true) {
                    inMill++;
                }
            }
        }

        return (pieces == inMill);
    }

    // Check if the game is terminated or not
    private char CheckTerminated(char[] board, int phase) {
        char winning = 'N';

        if (phase == 1) {
            return 'N';
        }

        if (CountPieces(board, 'B') <= 2) {
            winning = 'W';
        } else if (CountPieces(board, 'W') <= 2) {
            winning = 'B';
        } else if (CountMovablePieces(board, 'B', phase) == 0) {
            winning = 'W';
        } else if (CountMovablePieces(board, 'W', phase) == 0) {
            winning = 'B';
        }

        return winning;
    }

    // Count movable pieces on board
    private int CountMovablePieces(char[] board, char side, int phase) {
        int count = 0;
        bool isWeakSide = (CountPieces(board, side) == 3);

        if (phase == 3 && isWeakSide) {
            count = 3;
        } else {
            for (int i = 0; i < board.Length; i++) {
                if (board[i] == side) {
                    if (!CheckBlocked(board, i, gameController.GetAdjacentNode(i))) {
                        count++;
                    }
                }
            }
        }

        return count;
    }

    // Check if the current piece's adjacent node is empty
    private bool CheckBlocked(char[] board, int index, int[] adjNode) {
        bool blocked = true;

        for (int i = 0; i < adjNode.Length; i++) {
            if (board[adjNode[i]] == 'N') {
                blocked = false;
            }
        }

        return blocked;
    }

    // Evaluation function
    private double StateEvaluation(char[] board, int phase, int step, char side, double lastScore, bool difficulty) {
        double score = lastScore;
        int prefix = (side == 'B') ? 1 : -1;
        char opponent = gameController.getOppositeSide(side);
        int[][] coeficient = new int[3][] {
            new int[9] {18, 26, 1, 6, 12, 7, 0, 0, 0},
            new int[9] {14, 43, 10, 8, 0, 0, 7, 42, 1086},
            new int[9] {10, 0, 0, 0, 1, 16, 0, 0, 1190}
        };

        if (CheckFormedMill(board, step, side)) {
            score += prefix * coeficient[phase - 1][0];
        }

        int TotalMill = CountTotalMill(board, side);
        int BlockedPieces = CountPieces(board, opponent) - CountMovablePieces(board, opponent, phase);
        int TotalPieces = CountPieces(board, side);
        int TwoPiecesConfig = CountTwoPiecesConfiguration(board, side);
        int ThreePiecesConfig = CountThreePiecesConfiguration(board, side);
        int OpenedMill = CountOpenedMill(board, side, phase);
        int DoubleMill = CountDoubleMill(board, side);

        score += prefix * coeficient[phase - 1][1] * TotalMill;
        score += prefix * coeficient[phase - 1][2] * BlockedPieces;
        score += prefix * coeficient[phase - 1][3] * TotalPieces;

        if (difficulty) {
            score += prefix * coeficient[phase - 1][4] * TwoPiecesConfig;
            score += prefix * coeficient[phase - 1][5] * ThreePiecesConfig;
            score += prefix * coeficient[phase - 1][6] * OpenedMill;
            score += prefix * coeficient[phase - 1][7] * DoubleMill;
        }

        if (CheckTerminated(board, phase) == side) {
            score += prefix * coeficient[phase - 1][8];
        }

        return score;
    }

    // Check if the moved piece closed a mill
    private bool CheckFormedMill(char[] board, int placed, char side) {
        bool formed = false;
        int[][] millInfo = gameController.getPossibleMill();

        for (int i = 0; i < 16; i++) {
            if (millInfo[i][0] == placed || millInfo[i][1] == placed || millInfo[i][2] == placed) {
                if (board[millInfo[i][0]] == side && 
                    board[millInfo[i][0]] == board[millInfo[i][1]] && 
                    board[millInfo[i][0]] == board[millInfo[i][2]]) {
                        formed = true;
                    }
            }
        }

        return formed;
    }

    // Count total mill number
    private int CountTotalMill(char[] board, char side) {
        int count = 0;
        int[][] millInfo = gameController.getPossibleMill();

        for (int i = 0; i < 16; i++) {
            if (board[millInfo[i][0]] == side && board[millInfo[i][1]] == side && board[millInfo[i][2]] == side) {
                count++;
            }
        }

        return count;
    }

    // Count number of piece
    private int CountPieces(char[] board, char side) {
        int count = 0;
        for (int i = 0; i < board.Length; i++) {
            if (board[i] == side) {
                count++;
            }
        }

        return count;
    }

    // Count number of two pieces configuration
    private int CountTwoPiecesConfiguration(char[] board, char side) {
        int count = 0;
        int[][] millInfo = gameController.getPossibleMill();

        for (int x = 0; x < 16; x++) {
            int side_count = 0;
            int empty_count = 0;

            for (int y = 0; y < 3; y++) {
                if (board[millInfo[x][y]] == side) {
                    side_count++;
                } else if (board[millInfo[x][y]] == 'N') {
                    empty_count++;
                }
            }

            if (side_count == 2 && empty_count == 1) {
                count++;
            }
        }

        return count;
    }

    // Count number of three piece configuration
    private int CountThreePiecesConfiguration(char[] board, char side) {
        int count = 0;
        int[][] config = new int[44][] {
            new int[3] {0, 1, 9}, new int[3] {1, 2, 9}, new int[3] {1, 2, 4}, new int[3] {2, 4, 12},
            new int[3] {4, 7, 12}, new int[3] {4, 6, 7}, new int[3] {6, 7, 14}, new int[3] {5, 6, 14},
            new int[3] {3, 5, 6}, new int[3] {3, 5, 11}, new int[3] {0, 3, 11}, new int[3] {0, 1, 3},
            new int[3] {2, 8, 9}, new int[3] {8, 9, 17}, new int[3] {2, 9, 10}, new int[3] {9, 10, 17},
            new int[3] {9, 10, 12}, new int[3] {4, 10, 12}, new int[3] {10, 12, 20}, new int[3] {4, 12, 15},
            new int[3] {12, 15, 20}, new int[3] {12, 14, 15}, new int[3] {6, 14, 15}, new int[3] {14, 15, 22},
            new int[3] {6, 13, 14}, new int[3] {13, 14, 22}, new int[3] {11, 13, 14}, new int[3] {3, 11, 13},
            new int[3] {11, 13, 19}, new int[3] {3, 8, 11}, new int[3] {8, 11, 19}, new int[3] {8, 9, 11},
            new int[3] {9, 16, 17}, new int[3] {9, 17, 18}, new int[3] {17, 18, 20}, new int[3] {12, 18, 20},
            new int[3] {12, 20, 23}, new int[3] {20, 22, 23}, new int[3] {14, 22, 23}, new int[3] {14, 21, 22},
            new int[3] {19, 21, 22}, new int[3] {11, 19, 21}, new int[3] {11, 16, 19}, new int[3] {16, 17, 19}
        };

        int[][] configAdj = new int[44][] {
            new int[2] {2, 17}, new int[2] {0, 17}, new int[2] {0, 7}, new int[2] {7, 20},
            new int[2] {2, 20}, new int[2] {2, 5}, new int[2] {5, 22}, new int[2] {7, 22},
            new int[2] {0, 7}, new int[2] {0, 19}, new int[2] {5, 19}, new int[2] {2, 5},
            new int[2] {10, 17}, new int[2] {2, 10}, new int[2] {8, 17}, new int[2] {2, 8},
            new int[2] {8, 15}, new int[2] {15, 20}, new int[2] {4, 15}, new int[2] {10, 20},
            new int[2] {4, 10}, new int[2] {10, 13}, new int[2] {13, 22}, new int[2] {6, 13},
            new int[2] {15, 22}, new int[2] {6, 15}, new int[2] {8, 15}, new int[2] {8, 19},
            new int[2] {3, 8}, new int[2] {13, 19}, new int[2] {3, 13}, new int[2] {10, 13},
            new int[2] {1, 18}, new int[2] {1, 16}, new int[2] {16, 23}, new int[2] {4, 23},
            new int[2] {4, 18}, new int[2] {18, 21}, new int[2] {6, 21}, new int[2] {6, 23},
            new int[2] {16, 23}, new int[2] {3, 16}, new int[2] {3, 21}, new int[2] {18, 21}
        };

        for (int i = 0; i < 44; i++) {
            if (board[config[i][0]] == side && board[config[i][1]] == side && board[config[i][2]] == side) {
                if (board[configAdj[i][0]] == 'N' && board[configAdj[i][1]] == 'N') {
                    count++;
                }
            }
        }

        return count;
    }

    // Count open mill number
    private int CountOpenedMill(char[] board, char side, int phase) {
        int count = 0;
        int[][]millInfo = gameController.getPossibleMill();

        for (int x = 0; x < 16; x++) {
            for (int y = 0; y < 3; y++) {
                if (CheckPotentialMillFormation(board, millInfo[x][y], millInfo[x], phase, side)) {
                    count++;
                }
            }
        }

        return count;
    }

    // Count double mill number
    private int CountDoubleMill(char[] board, char side) {
        int count = 0;
        int[][] millInfo = gameController.getPossibleMill();

        for (int i = 0; i < board.Length; i++) {
            if (board[i] == side) {
                ArrayList checkNode = GetMillNodeInfo(i);
                bool doubled = true;

                foreach(int node in checkNode){
                    if (board[node] != side) {
                        doubled = false;
                    }
                }

                if (doubled) {
                    count++;
                }
            }
        }

        return count;
    }

    // Get mill info
    private ArrayList GetMillNodeInfo(int index) {
        ArrayList nodeList = new ArrayList();
        int[][] millInfo = gameController.getPossibleMill();

        for (int i = 0; i < 16; i++) {
            if (millInfo[i][0] == index || millInfo[i][1] == index || millInfo[i][2] == index) {
                nodeList.Add(millInfo[i][0]);
                nodeList.Add(millInfo[i][1]);
                nodeList.Add(millInfo[i][2]);
            }
        }

        return nodeList;
    }

    // Check if the mill is possible for closing mill
    private bool CheckPotentialMillFormation(char[] board, int placed, int[] mill, int phase, char side) {
        bool potential = false;
        bool isWeakSide = (CountPieces(board, side) == 3);
        
        if (mill[0] == placed && board[mill[0]] == 'N') {
            if (phase == 1 ||
                phase == 3 && isWeakSide ||
                phase == 2 && AdjacentContainPieces(board, side, gameController.GetAdjacentNode(mill[0]))) {
                if (board[mill[1]] == side && board[mill[2]] == side) {
                    potential = true;
                }
            }
        } else if (mill[1] == placed && board[mill[1]] == 'N') {
            if (phase == 1 ||
                phase == 3 && isWeakSide ||
                phase == 2 && AdjacentContainPieces(board, side, gameController.GetAdjacentNode(mill[1]))) {
                if (board[mill[0]] == side && board[mill[2]] == side) {
                    potential = true;
                }
            }
        } else if (mill[2] == placed && board[mill[2]] == 'N') {
            if (phase == 1 ||
                phase == 3 && isWeakSide ||
                phase == 2 && AdjacentContainPieces(board, side, gameController.GetAdjacentNode(mill[2]))) {
                if (board[mill[0]] == side && board[mill[1]] == side) {
                    potential = true;
                }
            }
        }

        return potential;
    }

    // Check if adjacent contain pieces
    private bool AdjacentContainPieces(char[] board, char side, int[] nodeList) {
        bool contain = false;
        int index = 0;

        while (!contain && index < nodeList.Length) {
            if (board[nodeList[index]] == side) {
                contain = true;
            }

            index++;
        }

        return contain;
    }

    // Get the index of spot on board
    private int GetPlacedPieces(char[] prev, char[] current) {
        int index = -1;
        for (int i = 0; i < prev.Length; i++) {
            if (prev[i] == 'N' && current[i] != 'N') {
                index = i;
            }
        }

        return index;
    }

    // Set removing piece
    public void SetRemovingToken(int index) {
        this.removeTokenIndex = index;
    }

    public int GetRemovingToken() { return this.removeTokenIndex; }
}