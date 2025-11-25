using Application.DTOs.Board.Board;
using Application.DTOs.Board.BoardMembership;
using FluentResults;

namespace Application.Interfaces.Board;

public interface IBoardService
{
    Task<Result<BoardDTO>> CreateBoard(CreateBoardDTO dto, int requesterId);
    Task<Result<BoardDTO>> GetBoard(int boardId, int requesterId);
    Task<Result<IQueryable<BoardDTO>>> GetBoards(int requesterId);

    Task<Result<BoardDTO>>
        UpdateBoard(int boardId, UpdateBoardDTO dto, int requesterId);

    Task<Result> DeactivateBoard(int boardId, int requesterId);

    Task<Result<BoardMembershipDTO>> CreateBoardMembership(int boardId,
        CreateBoardMembershipDTO dto, int requesterId);

    Task<Result<IQueryable<BoardMembershipDTO>>> GetBoardMemberships(int boardId,
        int requesterId);

    Task<Result<BoardMembershipDTO>> UpdateBoardMembership(int boardId,
        int userId,
        UpdateBoardMembershipDTO dto,
        int requesterId);

    Task<Result> DeleteBoardMembership(int boardId, int userId, int requesterId);
}
