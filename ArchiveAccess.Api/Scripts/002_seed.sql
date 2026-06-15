INSERT OR IGNORE INTO roles (id, code, name) VALUES
(1, 'user', 'Пользователь'),
(2, 'author', 'Автор документа'),
(3, 'approver', 'Согласующий'),
(4, 'admin', 'Администратор');

INSERT OR IGNORE INTO permissions (id, code, name) VALUES
(1, 'documents.read', 'Просмотр документов'),
(2, 'documents.create', 'Создание документов'),
(3, 'documents.approve', 'Согласование документов'),
(4, 'users.manage', 'Управление пользователями'),
(5, 'directories.manage', 'Управление справочниками');

INSERT OR IGNORE INTO role_permissions (role_id, permission_id) VALUES
(1, 1),
(2, 1),
(2, 2),
(3, 1),
(3, 3),
(4, 1),
(4, 2),
(4, 3),
(4, 4),
(4, 5);

INSERT OR IGNORE INTO users (id, username, full_name, password_hash, role_id, department) VALUES
(1, 'admin', 'Администратор системы', '3eb3fe66b31e3b4d10fa70b5cad49c7112294af6ae4e476a1c405155d45aa121', 4, 'Администрация'),
(2, 'ivanov', 'Иванов И.И.', '2f4816cb10348307a2238c64b2360681f0c5bd8ce3061a020f08e9565ec2ba36', 2, 'Отдел договоров'),
(3, 'petrov', 'Петров П.П.', '1e5af6febb8d22b08bdce6f956df3c08371c446aa1fa69414155920e02abdfb8', 3, 'Юридический отдел'),
(4, 'sidorova', 'Сидорова С.С.', '1e5af6febb8d22b08bdce6f956df3c08371c446aa1fa69414155920e02abdfb8', 3, 'Финансовый отдел'),
(5, 'user', 'Пользователь архива', 'bc5848f227cc161eb5f68dfe98cb13110a9c843ce69e953a88107d865583d397', 1, 'Общий отдел');

INSERT OR IGNORE INTO document_types (id, code, name) VALUES
(1, 'contract', 'Договор'),
(2, 'act', 'Акт'),
(3, 'regulation', 'Регламент'),
(4, 'instruction', 'Инструкция'),
(5, 'memo', 'Служебная записка');

INSERT OR IGNORE INTO document_statuses (id, code, name) VALUES
(1, 'created', 'Создан'),
(2, 'in_approval', 'На согласовании'),
(3, 'approved', 'Согласован'),
(4, 'rejected', 'Отклонен'),
(5, 'archived', 'Архивный'),
(6, 'revision', 'На доработке');

INSERT OR IGNORE INTO documents
(id, number, title, type_id, status_id, author_id, source_system, created_at, file_name, file_path)
VALUES
(1, 'ДГ-15/2024', 'Договор поставки № 15/2024 с ООО «Альфа»', 1, 2, 2, 'https://api1.example.com/documents', '2024-03-12', 'dogovor_15_2024.pdf', 'Files/dogovor_15_2024.pdf'),
(2, 'АКТ-7/2024', 'Акт выполненных работ № 7 от 25.04.2024', 2, 3, 2, 'https://api1.example.com/documents', '2024-04-25', 'akt_7_2024.pdf', 'Files/akt_7_2024.pdf'),
(3, 'РГ-3/2024', 'Регламент работы с входящей корреспонденцией', 3, 3, 2, 'https://api2.example.com/approvals', '2024-02-18', 'reglament_3_2024.pdf', 'Files/reglament_3_2024.pdf'),
(4, 'ПЛ-4/2024', 'Положение о порядке командировок', 3, 1, 2, 'https://api1.example.com/documents', '2024-05-03', 'polozhenie_4_2024.pdf', 'Files/polozhenie_4_2024.pdf'),
(5, 'ИН-10/2024', 'Инструкция по охране труда для офисных сотрудников', 4, 2, 2, 'https://api2.example.com/approvals', '2024-04-10', 'instruction_10_2024.pdf', 'Files/instruction_10_2024.pdf'),
(6, 'СЗ-22/2024', 'Служебная записка о приобретении оборудования', 5, 4, 2, 'https://api1.example.com/documents', '2024-04-22', 'memo_22_2024.pdf', 'Files/memo_22_2024.pdf');

INSERT OR IGNORE INTO approval_routes (id, document_id, name, source_system) VALUES
(1, 1, 'Маршрут согласования договора', 'https://api2.example.com/approvals'),
(2, 5, 'Маршрут согласования инструкции', 'https://api2.example.com/approvals');

INSERT OR IGNORE INTO approval_steps
(id, route_id, step_order, department, participant_user_id, status_id, decision_comment, decision_date)
VALUES
(1, 1, 1, 'Автор документа', 2, 3, 'Документ подготовлен.', '2024-03-12T09:15:00'),
(2, 1, 2, 'Юридический отдел', 3, 3, 'Замечаний нет. Согласовано.', '2024-03-13T10:32:00'),
(3, 1, 3, 'Финансовый отдел', 4, 2, NULL, NULL),
(4, 1, 4, 'Руководство', 1, 1, NULL, NULL),
(5, 2, 1, 'Автор документа', 2, 3, 'Документ подготовлен.', '2024-04-10T08:40:00'),
(6, 2, 2, 'Юридический отдел', 3, 2, NULL, NULL);