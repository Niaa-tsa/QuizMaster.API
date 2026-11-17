QuizMaster — Entity Relationship Diagram (ERD)
This diagram represents the EF Core data model configured in QuizMaster.Infrastructure/Data/QuizMasterDbContext.cs and the current model snapshot. It includes the core quiz workflow and the additional lecturer ownership relationship.
ER diagram
```mermaid
erDiagram
    ROLES ||--o{ USERS : assigns
    USERS o|--o{ QUIZZES : creates
    USERS ||--o{ QUIZ_ATTEMPTS : makes
    QUIZZES ||--o{ QUESTIONS : contains
    QUIZZES ||--o{ QUIZ_ATTEMPTS : has
    QUESTIONS ||--o{ ANSWER_OPTIONS : offers
    QUIZ_ATTEMPTS ||--o{ STUDENT_ANSWERS : contains
    QUESTIONS ||--o{ STUDENT_ANSWERS : answered_for
    ANSWER_OPTIONS ||--o{ STUDENT_ANSWERS : selected_option
    QUIZ_ATTEMPTS ||--o| QUIZ_RESULTS : produces

    ROLES {
        int Id PK
        string Name UK
    }

    USERS {
        int Id PK
        string UserName UK
        string Email UK
        string PasswordHash
        int RoleId FK
        string RequestedRole "nullable"
        string ApprovalStatus
        string ProfilePhotoUrl "nullable"
        int TokenVersion
        bool IsDeleted
    }

    QUIZZES {
        int Id PK
        string Title
        int TimeLimitMinutes
        int MaxAttempts
        bool IsPublished
        datetime CreatedAt
        int LecturerId FK "nullable"
    }

    QUESTIONS {
        int Id PK
        int QuizId FK
        string Text
        int Type
        decimal Points
    }

    ANSWER_OPTIONS {
        int Id PK
        int QuestionId FK
        string Text
        bool IsCorrect
    }

    QUIZ_ATTEMPTS {
        int Id PK
        int QuizId FK
        int UserId FK
        datetime StartedAt
        datetime FinishedAt "nullable"
        string Status
    }

    STUDENT_ANSWERS {
        int Id PK
        int AttemptId FK
        int QuestionId FK
        int AnswerOptionId FK
    }

    QUIZ_RESULTS {
        int Id PK
        int AttemptId FK_UK
        decimal Score
        decimal MaxScore
    }
```
Relationships
Parent table	Child table	Cardinality	Foreign key
Roles	Users	One-to-Many	Users.RoleId
Users	Quizzes	One-to-Many; quiz lecturer is optional	Quizzes.LecturerId (nullable)
Users	QuizAttempts	One-to-Many	QuizAttempts.UserId
Quizzes	Questions	One-to-Many	Questions.QuizId
Quizzes	QuizAttempts	One-to-Many	QuizAttempts.QuizId
Questions	AnswerOptions	One-to-Many	AnswerOptions.QuestionId
QuizAttempts	StudentAnswers	One-to-Many	StudentAnswers.AttemptId
Questions	StudentAnswers	One-to-Many	StudentAnswers.QuestionId
AnswerOptions	StudentAnswers	One-to-Many	StudentAnswers.AnswerOptionId
QuizAttempts	QuizResults	One-to-Zero-or-One	QuizResults.AttemptId (unique)


Important indexes and constraints
- Roles.Name — unique index.
- Users.UserName — unique index.
- Users.Email — unique index.
- QuizAttempts(QuizId, UserId) — unique filtered index for rows where Status = 'InProgress'; a student can have only one active attempt for a quiz at a time.
- QuizAttempts.QuizId — index.
- StudentAnswers(AttemptId, QuestionId, AnswerOptionId) — unique composite index.
- QuizResults.AttemptId — unique index, enforcing at most one result per attempt.
- Quizzes.TimeLimitMinutes must be greater than zero.
- Quizzes.MaxAttempts must be at least one.
- Questions.Points cannot be negative.
- QuizResults.Score and MaxScore cannot be negative, and Score cannot exceed MaxScore.
Delete behaviour
- Deleting a quiz cascades to its questions; deleting a question cascades to its answer options.
- Deleting a quiz or user that is referenced by quiz attempts is restricted.
- Deleting a quiz attempt cascades to its student answers.
- Deleting a question or answer option referenced by student answers is restricted.
- Deleting a quiz attempt while it has a quiz result is restricted.
- Deleting a role referenced by users is restricted.
Notes
- LecturerId is nullable in the current model, so a quiz can exist without an assigned lecturer. When present, it references Users.Id.
- The role table contains Lecturer and Student through EF Core model seeding. The startup DbSeeder also ensures SuperAdmin and Admin roles exist and creates the configured SuperAdmin user.
- Question.Type is stored as an integer enum (QuestionType).
- StudentAnswers stores one selected answer option per row. Multiple Choice responses are represented by multiple rows for the same attempt and question.
- StudentAnswers has separate foreign keys to the attempt, question and answer option. The current schema does not define a composite foreign key that enforces that the selected answer option belongs to the same question; this consistency is checked by application logic.