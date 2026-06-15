using ArchiveAccess.Api.Data;
using ArchiveAccess.Api.Dtos;
using ArchiveAccess.Api.Models;

namespace ArchiveAccess.Api.Repositories;

public sealed class ArchiveRepository : IArchiveRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public ArchiveRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<UserRecord?> GetUserByUsernameAsync(string username)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                u.id,
                u.username,
                u.full_name,
                u.password_hash,
                r.code AS role_code,
                r.name AS role_name,
                u.department
            FROM users u
            INNER JOIN roles r ON r.id = u.role_id
            WHERE lower(u.username) = lower($username)
            LIMIT 1;
            """;

        command.Parameters.AddWithValue("$username", username);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return ReadUser(reader);
    }

    public async Task<UserRecord?> GetUserByIdAsync(int userId)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                u.id,
                u.username,
                u.full_name,
                u.password_hash,
                r.code AS role_code,
                r.name AS role_name,
                u.department
            FROM users u
            INNER JOIN roles r ON r.id = u.role_id
            WHERE u.id = $id
            LIMIT 1;
            """;

        command.Parameters.AddWithValue("$id", userId);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return ReadUser(reader);
    }

    public async Task<IReadOnlyList<DocumentListItemDto>> GetDocumentsAsync()
    {
        var result = new List<DocumentListItemDto>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                d.id,
                d.number,
                d.title,
                dt.name AS type_name,
                ds.name AS status_name,
                ds.code AS status_code,
                u.full_name AS author_name,
                u.department,
                d.source_system,
                d.created_at,
                d.file_name
            FROM documents d
            INNER JOIN document_types dt ON dt.id = d.type_id
            INNER JOIN document_statuses ds ON ds.id = d.status_id
            INNER JOIN users u ON u.id = d.author_id
            ORDER BY d.created_at DESC, d.id DESC;
            """;

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(ReadDocumentListItem(reader));
        }

        return result;
    }

    public async Task<DocumentCardDto?> GetDocumentCardAsync(int documentId)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                d.id,
                d.number,
                d.title,
                dt.name AS type_name,
                ds.name AS status_name,
                ds.code AS status_code,
                u.full_name AS author_name,
                u.department,
                d.source_system,
                d.created_at,
                d.file_name
            FROM documents d
            INNER JOIN document_types dt ON dt.id = d.type_id
            INNER JOIN document_statuses ds ON ds.id = d.status_id
            INNER JOIN users u ON u.id = d.author_id
            WHERE d.id = $id
            LIMIT 1;
            """;

        command.Parameters.AddWithValue("$id", documentId);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        var baseDocument = ReadDocumentListItem(reader);
        var steps = await GetApprovalStepsAsync(documentId);

        return new DocumentCardDto
        {
            Id = baseDocument.Id,
            Number = baseDocument.Number,
            Title = baseDocument.Title,
            Type = baseDocument.Type,
            Status = baseDocument.Status,
            StatusCode = baseDocument.StatusCode,
            Author = baseDocument.Author,
            Department = baseDocument.Department,
            SourceSystem = baseDocument.SourceSystem,
            CreatedAt = baseDocument.CreatedAt,
            FileName = baseDocument.FileName,
            ApprovalSteps = steps
        };
    }

    public async Task<IReadOnlyList<ApprovalStepDto>> GetApprovalStepsAsync(int documentId)
    {
        var result = new List<ApprovalStepDto>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                s.id,
                s.step_order,
                s.department,
                u.full_name AS participant_name,
                u.id AS participant_user_id,
                ds.name AS status_name,
                ds.code AS status_code,
                s.decision_comment,
                s.decision_date
            FROM approval_steps s
            INNER JOIN approval_routes r ON r.id = s.route_id
            INNER JOIN users u ON u.id = s.participant_user_id
            INNER JOIN document_statuses ds ON ds.id = s.status_id
            WHERE r.document_id = $document_id
            ORDER BY s.step_order;
            """;

        command.Parameters.AddWithValue("$document_id", documentId);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new ApprovalStepDto
            {
                Id = reader.GetInt32(0),
                StepOrder = reader.GetInt32(1),
                Department = reader.GetString(2),
                Participant = reader.GetString(3),
                ParticipantUserId = reader.GetInt32(4),
                Status = reader.GetString(5),
                StatusCode = reader.GetString(6),
                DecisionComment = reader.IsDBNull(7) ? null : reader.GetString(7),
                DecisionDate = reader.IsDBNull(8) ? null : reader.GetString(8)
            });
        }

        return result;
    }

    public async Task<ApprovalStepRecord?> GetCurrentPendingStepAsync(int documentId)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT
                s.id,
                r.document_id,
                s.participant_user_id,
                s.step_order,
                ds.code AS status_code
            FROM approval_steps s
            INNER JOIN approval_routes r ON r.id = s.route_id
            INNER JOIN document_statuses ds ON ds.id = s.status_id
            WHERE r.document_id = $document_id
              AND ds.code = 'in_approval'
            ORDER BY s.step_order
            LIMIT 1;
            """;

        command.Parameters.AddWithValue("$document_id", documentId);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        return new ApprovalStepRecord
        {
            Id = reader.GetInt32(0),
            DocumentId = reader.GetInt32(1),
            ParticipantUserId = reader.GetInt32(2),
            StepOrder = reader.GetInt32(3),
            StatusCode = reader.GetString(4)
        };
    }

    public async Task UpdateApprovalStepAsync(
        int stepId,
        string decisionStatusCode,
        string? comment)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE approval_steps
            SET
                status_id = (SELECT id FROM document_statuses WHERE code = $status_code),
                decision_comment = $comment,
                decision_date = $decision_date
            WHERE id = $step_id;
            """;

        command.Parameters.AddWithValue("$step_id", stepId);
        command.Parameters.AddWithValue("$status_code", decisionStatusCode);
        command.Parameters.AddWithValue("$comment", comment ?? string.Empty);
        command.Parameters.AddWithValue("$decision_date", DateTime.UtcNow.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateDocumentStatusAsync(
        int documentId,
        string statusCode)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE documents
            SET status_id = (SELECT id FROM document_statuses WHERE code = $status_code)
            WHERE id = $document_id;
            """;

        command.Parameters.AddWithValue("$document_id", documentId);
        command.Parameters.AddWithValue("$status_code", statusCode);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<IReadOnlyList<DirectoryValueDto>> GetDocumentTypesAsync()
    {
        return await GetDirectoryAsync("document_types");
    }

    public async Task<IReadOnlyList<DirectoryValueDto>> GetDocumentStatusesAsync()
    {
        return await GetDirectoryAsync("document_statuses");
    }

    public async Task<int> CountDocumentsAsync()
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM documents;";

        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }

    public async Task AddAuditLogAsync(
        int? userId,
        string action,
        string entityType,
        int? entityId,
        string? details)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO audit_logs
                (user_id, action, entity_type, entity_id, details, created_at)
            VALUES
                ($user_id, $action, $entity_type, $entity_id, $details, $created_at);
            """;

        command.Parameters.AddWithValue("$user_id", userId.HasValue ? userId.Value : DBNull.Value);
        command.Parameters.AddWithValue("$action", action);
        command.Parameters.AddWithValue("$entity_type", entityType);
        command.Parameters.AddWithValue("$entity_id", entityId.HasValue ? entityId.Value : DBNull.Value);
        command.Parameters.AddWithValue("$details", details ?? string.Empty);
        command.Parameters.AddWithValue("$created_at", DateTime.UtcNow.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    private async Task<IReadOnlyList<DirectoryValueDto>> GetDirectoryAsync(string tableName)
    {
        var result = new List<DirectoryValueDto>();

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT code, name FROM {tableName} ORDER BY name;";

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(new DirectoryValueDto(
                reader.GetString(0),
                reader.GetString(1)));
        }

        return result;
    }

    private static UserRecord ReadUser(System.Data.Common.DbDataReader reader)
    {
        return new UserRecord
        {
            Id = reader.GetInt32(0),
            Username = reader.GetString(1),
            FullName = reader.GetString(2),
            PasswordHash = reader.GetString(3),
            RoleCode = reader.GetString(4),
            RoleName = reader.GetString(5),
            Department = reader.GetString(6)
        };
    }

    private static DocumentListItemDto ReadDocumentListItem(System.Data.Common.DbDataReader reader)
    {
        return new DocumentListItemDto
        {
            Id = reader.GetInt32(0),
            Number = reader.GetString(1),
            Title = reader.GetString(2),
            Type = reader.GetString(3),
            Status = reader.GetString(4),
            StatusCode = reader.GetString(5),
            Author = reader.GetString(6),
            Department = reader.GetString(7),
            SourceSystem = reader.GetString(8),
            CreatedAt = reader.GetString(9),
            FileName = reader.IsDBNull(10) ? null : reader.GetString(10)
        };
    }
}