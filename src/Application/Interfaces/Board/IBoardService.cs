using Application.DTOs.Board;
using Application.DTOs.Board.Board;
using Application.DTOs.Board.BoardMembership;
using FluentResults;

namespace Application.Interfaces.Board;

public interface IBoardService
{
    Task<Result<BoardDTO>> CreateBoard(CreateBoardDTO dto, int userId);
    Task<Result<BoardDTO>> GetBoard(int boardId, int requesterId);
    Task<Result<IEnumerable<BoardDTO>>> GetBoards(int requesterId);

    Task<Result<BoardDTO>>
        UpdateBoard(int boardId, UpdateBoardDTO dto, int requesterId);

    Task<Result> DeactivateBoard(int boardId, int requesterId);

    Task<Result<BoardMembershipDTO>> CreateBoardMembership(int boardId,
        CreateBoardMembershipDTO dto, int requesterId);

    Task<Result<IEnumerable<BoardMembershipDTO>>> GetBoardMemberships(int boardId,
        int requesterId);

    Task<Result<BoardMembershipDTO>> UpdateBoardMembership(int membershipId,
        UpdateBoardMembershipDTO dto,
        int requesterId);

    Task<Result> RemoveBoardMembership(int membershipId, int requesterId);
}
