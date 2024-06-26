﻿using System.Data.SQLite;
using System.IO;

namespace protocolPlus.Core
{
    internal class DatabaseUtils
    {
        public static void InitDatabase()
        {
            var directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dbFile = Path.Combine(directory, "energosfera.sqlite");

            var dbConnection = new SQLiteConnection($"Data Source={dbFile}");

            if (!File.Exists(dbFile))
            {
                CreateDB(dbConnection);
                InitialFillDB(dbConnection);
            }

            try
            {
                dbConnection.Open();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
            finally
            {
                dbConnection.Close();
            }
        }

        public static SQLiteConnection CreateConnection()
        {
            var directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dbFile = Path.Combine(directory, "energosfera.sqlite");
            var dbConnection = new SQLiteConnection($"Data Source={dbFile}");

            return dbConnection;
        }

        public static void CreateDB(SQLiteConnection dbConnection)
        {
            dbConnection.Open();



            var createTable = dbConnection.CreateCommand();
            createTable.CommandText =
                @"
                BEGIN TRANSACTION;

                CREATE TABLE IF NOT EXISTS tool (
                  id INTEGER PRIMARY KEY,
                  name TEXT,
                  type TEXT,
                  assurance_num TEXT,
                  verification_num TEXT,
                  verification_date TEXT
                );

                CREATE TABLE IF NOT EXISTS machine (
                  id INTEGER PRIMARY KEY,
                  type INTEGER,
                  name TEXT,
                  assurance_num TEXT,
                  power TEXT,
                  voltage_st TEXT,
                  current_st TEXT,
                  frequency TEXT,
                  rpm TEXT,
                  cosinus TEXT,
                  efficency TEXT,
                  current_exc TEXT,
                  voltage_exc TEXT,
                  rotation TEXT,
                  stator_num TEXT,
                  rotor_num TEXT
                );

                CREATE TABLE IF NOT EXISTS employee (
                  id INTEGER PRIMARY KEY,
                  name TEXT NOT NULL,
                  position TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS revisionGroup (
                  id INTEGER PRIMARY KEY,
                  name TEXT
                );

                CREATE TABLE IF NOT EXISTS revisionResult (
                  id INTEGER PRIMARY KEY,
                  tag TEXT, 
                  permissible_value TEXT,
                  check_type INTEGER
                );

                CREATE TABLE IF NOT EXISTS revisionCell (
                  id INTEGER PRIMARY KEY,
                  name TEXT,
                  tag TEXT, 
                  units TEXT,
                  revisionResult_id INTEGER,
                  revisionGroup_id INTEGER,
                  is_checked INTEGER,
                  FOREIGN KEY (revisionResult_id) REFERENCES revisionResult (id),
                  FOREIGN KEY (revisionGroup_id) REFERENCES revisionGroup (id)
                );

                CREATE TABLE IF NOT EXISTS protocol_template (
                  id INTEGER PRIMARY key,
                  name TEXT,
                  description TEXT,
                  path_to_file TEXT,
                  update_date TEXT
                );

                CREATE TABLE IF NOT EXISTS revisions_in_protocols (
                  protocol_template_id INTEGER NOT NULL,
                  revision_id INTEGER NOT NULL,
                  FOREIGN KEY (protocol_template_id) REFERENCES protocol_template (id)
                  FOREIGN key (revision_id) REFERENCES revision (id)
                );

                CREATE TABLE IF not EXISTS protocol (
                  id INTEGER PRIMARY key,
                  protocol_num TEXT NOT NULL,
                  creation_date TEXT NOT NULL,
                  conclusion TEXT NOT NULL,
                  protocol_template_id INTEGER NOT NULL,
                  machine_id INTEGER NOT NULL,
                  employee_id INTEGER NOT NULL,
                  FOREIGN key (protocol_template_id) REFERENCES protocol_template (id)
                  FOREIGN key (machine_id) REFERENCES machine (id)
                  FOREIGN KEY (employee_id) REFERENCES employee (id)
                );

                CREATE TABLE IF NOT EXISTS tools_in_protocols (
                  protocol_id INTEGER NOT NULL,
                  tool_id INTEGER NOT NULL,
                  FOREIGN KEY (protocol_id) REFERENCES protocol (id)
                  FOREIGN key (tool_id) REFERENCES tool (id)
                );

                COMMIT;
            ";
            createTable.ExecuteNonQuery();

            dbConnection.Close();
        }

        public static void InitialFillDB(SQLiteConnection dbConnection)
        {
            dbConnection.Open();

            var insertValues = dbConnection.CreateCommand();
            insertValues.CommandText =
                @"
                BEGIN TRANSACTION;

                INSERT into tool 
	                (name, type, assurance_num, verification_num, verification_date) 
	                VALUES 	
    	                ('Мегаомметр', 'Е6-32', '502', 'С-ВН/01-03-2023/227156495', '28-02-2025'),
    	                ('иллиомметр', 'ТФ-1', '1062', 'С-ВН/12-04-2023/238472547', '11-04-2024'),
    	                ('астотомер цифровой', 'СС3020-Н', '657', 'С-ВН/14-02-2022/131927510', '13-02-2024'),
    	                ('ольтметр', 'Э545', '722', 'С-ВН/14-06-2023/254225090', '13-06-2024'),
    	                ('Амперметр', 'Э538', '3953', 'С-ВН/13-04-2023/238725114', '12-04-2024'),
    	                ('Вольтамперметр', 'М2044', '4237', 'С-ВН/06-03-2023/228363823', '05-03-2024');
        
                INSERT INTO machine 
	                (type, 
                     name, 
                     assurance_num, 
                     power, 
                     voltage_st,
                     current_st,
                     frequency,
                     rpm,
                     cosinus,
                     efficency,
                     current_exc,
                     voltage_exc,
                     rotation
                     )
                     VALUES 
     	                ('СТД', 'СТДу-3150-2ЗУХЛ4', '00017/23', 3150, 6000, 352, 50, 3000, 0.9, 97.6, 262, 89, 'левое'),
                        ('СТД', 'СТДу-2355-22УХЛ5', '00532/22', 2355, 3232, 634, 76, 1242, 2.4, 99.23, 546, 32, 'правое');

                INSERT INTO revisionGroup (
                  name)
                  VALUES
                   ('Измерение сопротивления изоляции'), 
                   ('Испытания электрической прочности изоляции'), 
                   ('Измерение сопротивления обмоток постоянному току при t = 15ºС'), 
                   ('Измерение биения контактных колец'), 
                   ('Измерение проверки совмещения магнитных осей'), 
                   ('Проверка работы двигателя на холостом ходу'), 
                   ('Измерение вибрации подшипниковых опор'), 
                   ('Проверка заложенных термопреобразователей сопротивления');

                INSERT into revisionResult (
                  tag, 
                  permissible_value,
                  check_type)
                  VALUES 
  	               ('<1.1.result>', '60', 1),      
                   ('<1.2.result>', '1,3', 1),     
                   ('<1.3.result>', '0,2', 1),     
                   ('<1.4.result>', '1', 1),       
                   ('<2.1.result>', '100', 1),       
                   ('<2.2.result>', '100', 1),     
                   ('<2.3.result>', '100', 1),     
                   ('<2.4.result>', '100', 1),     
                   ('<3.1.result>', '2', 3),       
                   ('<3.2.result>', '0,25', 2),    
                   ('<3.3.result>', '2', 3),       
                   ('<3.4.result>', '0,5', 2),     
                   ('<3.5.result>', '0,00006', 2), 
                   ('<4.1.result>', '0,03', 5),
                   ('<4.2.result>', '0,03', 5),
                   ('<4.3.result>', '5', 5),
                   ('<4.4.result>', '5', 5),
                   ('<7.2.result>', '80', 2),
                   ('<7.3.result>', '2,3', 2),
                   ('<8.1.result>', '50', 6),
                   ('<8.2.result>', '50', 6),
                   ('<8.3.result>', '50', 6),
                   ('<8.4.result>', '50', 6);

                INSERT INTO revisionCell (
                  name,
                  tag, 
                  units,
                  is_checked,
                  revisionResult_id, 
                  revisionGroup_id)
                  VALUES
                    ('обмотка статора: U-VW', '<1.1.1>', 'МОм', 1, 1, 1),
                    ('обмотка статора: V-WU', '<1.1.2>', 'МОм', 1, 1, 1),
                    ('обмотка статора: W-UV', '<1.1.3>', 'МОм', 1, 1, 1),
                    ('обмотка статора: U', '<1.1.4>', 'МОм', 1, 1, 1),
                    ('обмотка статора: V', '<1.1.5>', 'МОм', 1, 1, 1),
                    ('обмотка статора: W', '<1.1.6>', 'МОм', 1, 1, 1),
                    ('Коэффициент абсорбции: U-VW', '<1.2.1>', 0, 1, 2, 1),
                    ('Коэффициент абсорбции: V-WU', '<1.2.2>', 0, 1, 2, 1),
                    ('Коэффициент абсорбции: W-UV', '<1.2.3>', 0, 1, 2, 1),
                    ('Коэффициент абсорбции: U', '<1.2.4>', 0, 1, 2, 1),
                    ('Коэффициент абсорбции: V', '<1.2.5>', 0, 1, 2, 1),
                    ('Коэффициент абсорбции: W', '<1.2.6>', 0, 1, 2, 1),
                    ('Обмотка ротора', '<1.3>', 'МОм', 1, 3, 1),
                    ('Стояк - плита', '<1.4>', 'МОм', 1, 4, 1),
                    ('Обмоток фаз статора относительно корпуса и относительно друг друга. Фаза U. R до испытаний', '<2.1.1>', 'МОм', 0, NULL, 2),
                    ('Обмоток фаз статора относительно корпуса и относительно друг друга. Фаза U. Напряжение испытаний', '<2.1.2>', 'В', 0, NULL, 2),
                    ('Обмоток фаз статора относительно корпуса и относительно друг друга. Фаза U. Длительность испытаний', '<2.1.3>', 'секунды', 0, NULL, 2),
                    ('Обмоток фаз статора относительно корпуса и относительно друг друга. Фаза U. R после испытаний', '<2.1.4>', 'МОм', 1, 5, 2),
                    ('Обмоток фаз статора относительно корпуса и относительно друг друга. Фаза V. R до испытаний', '<2.2.1>', 'МОм', 0, NULL, 2),
                    ('Обмоток фаз статора относительно корпуса и относительно друг друга. Фаза V. Напряжение испытаний', '<2.2.2>', 'В', 0, NULL, 2),
                    ('Обмоток фаз статора относительно корпуса и относительно друг друга. Фаза V. Длительность испытаний', '<2.2.3>', 'секунды', 0, NULL, 2),
                    ('Обмоток фаз статора относительно корпуса и относительно друг друга. Фаза V. R после испытаний', '<2.2.4>', 'МОм', 1, 6, 2),
                    ('Обмоток фаз статора относительно корпуса и относительно друг друга. Фаза W. R до испытаний', '<2.3.1>', 'МОм', 0, NULL, 2),
                    ('Обмоток фаз статора относительно корпуса и относительно друг друга. Фаза W. Напряжение испытаний', '<2.3.2>', 'В', 0, NULL, 2),
                    ('Обмоток фаз статора относительно корпуса и относительно друг друга. Фаза W. Длительность испытаний', '<2.3.3>', 'секунды', 0, NULL, 2),
                    ('Обмоток фаз статора относительно корпуса и относительно друг друга. Фаза W. R после испытаний', '<2.3.4>', 'МОм', 1, 7, 2),
                    ('Ротор. R до испытаний', '<2.4.1>', 'МОм', 0, NULL, 2),
                    ('Ротор. Напряжение испытаний', '<2.4.2>', 'В', 0, NULL, 2),
                    ('Ротор. Длительность испытаний', '<2.4.3>', 'секунды', 0, NULL, 2),
                    ('Ротор. R после испытаний', '<2.4.4>', 'МОм', 1, 8, 2),
                    ('Статор. U', '<3.1.1>', 'Ом', 1, 9, 3),
                    ('Статор. V', '<3.1.2>', 'Ом', 1, 9, 3),
                    ('Статор. W', '<3.1.3>', 'Ом', 1, 9, 3),
                    ('Ротор', '<3.2>', 'Ом', 1, 10, 3),
                    ('Якорь возбудителя. U', '<3.3.1>', 'Ом', 1, 11, 3),
                    ('Якорь возбудителя. V', '<3.3.2>', 'Ом', 1, 11, 3),
                    ('Якорь возбудителя. W', '<3.3.3>', 'Ом', 1, 11, 3),
                    ('Индуктор возбудителя', '<3.4>', 'Ом', 1, 12, 3),
                    ('Между заз. болтом и корпусом', '<3.5>', 'Ом', 1, 13, 3),
                    ('Наружное', '<4.1>', 'мм', 1, 14, 4),
                    ('Внутреннее', '<4.2>', 'мм', 1, 15, 4),
                    ('Т', '<4.3>', 'мм', 1, 16, 5),
                    ('В', '<4.4>', 'мм', 1, 17, 5),
                    ('Напряжение', '<6.1>', 'В', 0, NULL, 6),
                    ('Ток возбуждения', '<6.2>', 'А', 0, NULL, 6),
                    ('Частота', '<6.3>', 'Гц', 0, NULL, 6),
                    ('Время обкатки', '<6.4>', 'мин', 0, NULL, 6),
                    ('Температура подшипников. Сторона возбуждения', '<6.5>', 'ºС', 0, NULL, 6),
                    ('Частота вращения', '<7.1.1>', 'об/мин', 0, NULL, 7),
                    ('Время', '<7.1.2>', 'мин', 0, NULL, 7),
                    ('Температура подшипников. Подшипник 1', '<7.2.1>', 'ºС', 1, 18, 7),
                    ('Температура подшипников. Подшипник 2', '<7.2.2>', 'ºС', 1, 18, 7),
                    ('СКЗ виброскорости. Сторона привода 1 X', '<7.3.1>', 'мм/с', 1, 19, 7),
                    ('СКЗ виброскорости. Сторона привода 2 X', '<7.3.2>', 'мм/с', 1, 19, 7),
                    ('СКЗ виброскорости. Сторона привода 3 X', '<7.3.3>', 'мм/с', 1, 19, 7),
                    ('СКЗ виброскорости. Сторона возбуждения 4 X', '<7.3.4>', 'мм/с', 1, 19, 7),
                    ('СКЗ виброскорости. Сторона возбуждения 5 X', '<7.3.5>', 'мм/с', 1, 19, 7),
                    ('СКЗ виброскорости. Сторона возбуждения 6 X', '<7.3.6>', 'мм/с', 1, 19, 7),
                    ('Железо и обмотка статора. №п/п 1. Rа при t =15 ºС', '<8.1.1>', 'Ом', 1, 20, 8),
                    ('Железо и обмотка статора. №п/п 1. Rиз', '<8.1.2>', 'МОм', 0, NULL, 8),
                    ('Железо и обмотка статора. №п/п 2. Rа при t =15 ºС', '<8.1.3>', 'Ом', 1, 20, 8),
                    ('Железо и обмотка статора. №п/п 2. Rиз', '<8.1.4>', 'МОм', 0, NULL, 8),
                    ('Железо и обмотка статора. №п/п 3. Rа при t =15 ºС', '<8.1.5>', 'Ом', 1, 20, 8),
                    ('Железо и обмотка статора. №п/п 3. Rиз', '<8.1.6>', 'МОм', 0, NULL, 8),
                    ('Железо и обмотка статора. №п/п 4. Rа при t =15 ºС', '<8.1.7>', 'Ом', 1, 20, 8),
                    ('Железо и обмотка статора. №п/п 4. Rиз', '<8.1.8>', 'МОм', 0, NULL, 8),
                    ('Железо и обмотка статора. №п/п 5. Rа при t =15 ºС', '<8.1.9>', 'Ом', 1, 20, 8),
                    ('Железо и обмотка статора. №п/п 5. Rиз', '<8.1.10>', 'МОм', 0, NULL, 8),
                    ('Железо и обмотка статора. №п/п 6. Rа при t =15 ºС', '<8.1.11>', 'Ом', 1, 20, 8),
                    ('Железо и обмотка статора. №п/п 6. Rиз', '<8.1.12>', 'МОм', 0, NULL, 8),
                    ('Воздух. №п/п 7. Rа при t =15 ºС', '<8.2.1>', 'Ом', 1, 21, 8),
                    ('Воздух. №п/п 7. Rиз', '<8.2.2>', 'МОм', 0, NULL, 8),
                    ('Воздух. №п/п 8. Rа при t =15 ºС', '<8.2.3>', 'Ом', 1, 21, 8),
                    ('Воздух. №п/п 8. Rиз', '<8.2.4>', 'МОм', 0, NULL, 8),
                    ('Воздух. №п/п 9. Rа при t =15 ºС', '<8.2.5>', 'Ом', 1, 21, 8),
                    ('Воздух. №п/п 9. Rиз', '<8.2.6>', 'МОм', 0, NULL, 8),
                    ('Воздух. №п/п 10. Rа при t =15 ºС', '<8.2.7>', 'Ом', 1, 21, 8),
                    ('Воздух. №п/п 10. Rиз', '<8.2.8>', 'МОм', 0, NULL, 8),
                    ('Подшипник. Со ст. рабочего конца вала. Rа при t =15 ºС', '<8.3.1>', 'Ом', 1, 22, 8),
                    ('Подшипник. Со ст. рабочего конца вала. Rиз', '<8.3.2>', 'МОм', 0, NULL, 8),
                    ('Подшипник. Со ст. возбуждения. Rа при t =15 ºС', '<8.3.3>', 'Ом', 1, 22, 8),
                    ('Подшипник. Со ст. возбуждения. Rиз', '<8.3.4>', 'МОм', 0, NULL, 8),
                    ('Масло. Со ст. рабочего конца вала. Rа при t =15 ºС', '<8.4.1>', 'Ом', 1, 23, 8),
                    ('Масло. Со ст. рабочего конца вала. Rиз', '<8.4.2>', 'МОм', 0, NULL, 8),
                    ('Масло. Со ст. возбуждения. Rа при t =15 ºС', '<8.4.3>', 'Ом', 1, 23, 8),
                    ('Масло. Со ст. возбуждения. Rиз', '<8.4.4>', 'МОм', 0, NULL, 8);
    

                COMMIT;
            ";
            insertValues.ExecuteNonQuery();

            dbConnection.Close();
        }

        public static int GetRevisions(SQLiteConnection dbConnection, string table)
        {
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = $"SELECT revision.name, revision_cell.name, revision_cell.units, revision.permissible_value, revision.check_type, revision_cell.is_checked, revision_cell.tag, revision.result_tag\r\nFROm revision_cell INNER JOIN revision ON revision.id = revision_cell.revision_id";

            long columns = (long)cmd.ExecuteScalar();

            int result = (int)columns;

            return result;
        }

        public static int GetColumns(SQLiteConnection dbConnection, string table)
        {
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = $"SELECT COUNT(*) FROM pragma_table_info('{table}');;";

            long columns = (long)cmd.ExecuteScalar();

            int result = (int)columns;

            return result;
        }

        public static void PrintTable(SQLiteConnection dbConnection, string table)
        {
            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = $"SELECT * FROM {table};";

            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                for (int i = 1; i < reader.FieldCount; i++)
                {
                    if (!reader.IsDBNull(i))
                        Console.Write(reader.GetString(i) + " ");
                }
                Console.WriteLine();
            }
            reader.Close();
        }
    }
}
