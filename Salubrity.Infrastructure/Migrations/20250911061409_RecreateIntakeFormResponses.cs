using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Salubrity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RecreateIntakeFormResponses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create the table if it does not exist
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""IntakeFormResponses"" (
                    ""Id"" uuid NOT NULL PRIMARY KEY,
                    ""IntakeFormVersionId"" uuid NOT NULL,
                    ""SubmittedByUserId"" uuid NOT NULL,
                    ""PatientId"" uuid NOT NULL,
                    ""SubmittedServiceId"" uuid NOT NULL,
                    ""SubmittedServiceType"" integer NOT NULL,
                    ""ResolvedServiceId"" uuid NOT NULL,
                    ""ResponseStatusId"" uuid NOT NULL,
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    ""UpdatedAt"" timestamp with time zone NULL,
                    ""DeletedAt"" timestamp with time zone NULL,
                    ""IsDeleted"" boolean NOT NULL,
                    ""CreatedBy"" uuid NULL,
                    ""UpdatedBy"" uuid NULL,
                    ""DeletedBy"" uuid NULL
                );
            ");

            // 2. Add missing columns
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                   WHERE table_name='IntakeFormResponses' AND column_name='SubmittedByUserId') THEN
                        ALTER TABLE ""IntakeFormResponses"" ADD COLUMN ""SubmittedByUserId"" uuid NOT NULL DEFAULT gen_random_uuid();
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                   WHERE table_name='IntakeFormResponses' AND column_name='SubmittedServiceId') THEN
                        ALTER TABLE ""IntakeFormResponses"" ADD COLUMN ""SubmittedServiceId"" uuid NOT NULL DEFAULT gen_random_uuid();
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                   WHERE table_name='IntakeFormResponses' AND column_name='SubmittedServiceType') THEN
                        ALTER TABLE ""IntakeFormResponses"" ADD COLUMN ""SubmittedServiceType"" integer NOT NULL DEFAULT 0;
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                   WHERE table_name='IntakeFormResponses' AND column_name='ResolvedServiceId') THEN
                        ALTER TABLE ""IntakeFormResponses"" ADD COLUMN ""ResolvedServiceId"" uuid NOT NULL DEFAULT gen_random_uuid();
                    END IF;
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns 
                                   WHERE table_name='IntakeFormResponses' AND column_name='ResponseStatusId') THEN
                        ALTER TABLE ""IntakeFormResponses"" ADD COLUMN ""ResponseStatusId"" uuid NOT NULL DEFAULT gen_random_uuid();
                    END IF;
                END$$;
            ");

            // 3. Add indexes if missing
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_class c
                        JOIN pg_namespace n ON n.oid = c.relnamespace
                        WHERE c.relname = 'ix_intakeformresponses_intakeformversionid'
                          AND n.nspname = 'public'
                    ) THEN
                        CREATE INDEX ""IX_IntakeFormResponses_IntakeFormVersionId"" 
                        ON ""IntakeFormResponses""(""IntakeFormVersionId"");
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1 FROM pg_class c
                        JOIN pg_namespace n ON n.oid = c.relnamespace
                        WHERE c.relname = 'ix_intakeformresponses_patientid'
                          AND n.nspname = 'public'
                    ) THEN
                        CREATE INDEX ""IX_IntakeFormResponses_PatientId"" 
                        ON ""IntakeFormResponses""(""PatientId"");
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1 FROM pg_class c
                        JOIN pg_namespace n ON n.oid = c.relnamespace
                        WHERE c.relname = 'ix_intakeformresponses_resolvedserviceid'
                          AND n.nspname = 'public'
                    ) THEN
                        CREATE INDEX ""IX_IntakeFormResponses_ResolvedServiceId"" 
                        ON ""IntakeFormResponses""(""ResolvedServiceId"");
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1 FROM pg_class c
                        JOIN pg_namespace n ON n.oid = c.relnamespace
                        WHERE c.relname = 'ix_intakeformresponses_responsestatusid'
                          AND n.nspname = 'public'
                    ) THEN
                        CREATE INDEX ""IX_IntakeFormResponses_ResponseStatusId"" 
                        ON ""IntakeFormResponses""(""ResponseStatusId"");
                    END IF;
                END$$;
            ");

            // 4. Add foreign keys if missing
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_constraint WHERE conname = 'fk_intakeformresponses_intakeformversions_intakeformversionid'
                    ) THEN
                        ALTER TABLE ""IntakeFormResponses"" ADD CONSTRAINT ""FK_IntakeFormResponses_IntakeFormVersions_IntakeFormVersionId""
                        FOREIGN KEY (""IntakeFormVersionId"") REFERENCES ""IntakeFormVersions""(""Id"") ON DELETE CASCADE;
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1 FROM pg_constraint WHERE conname = 'fk_intakeformresponses_patients_patientid'
                    ) THEN
                        ALTER TABLE ""IntakeFormResponses"" ADD CONSTRAINT ""FK_IntakeFormResponses_Patients_PatientId""
                        FOREIGN KEY (""PatientId"") REFERENCES ""Patients""(""Id"") ON DELETE CASCADE;
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1 FROM pg_constraint WHERE conname = 'fk_intakeformresponses_services_resolvedserviceid'
                    ) THEN
                        ALTER TABLE ""IntakeFormResponses"" ADD CONSTRAINT ""FK_IntakeFormResponses_Services_ResolvedServiceId""
                        FOREIGN KEY (""ResolvedServiceId"") REFERENCES ""Services""(""Id"") ON DELETE CASCADE;
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1 FROM pg_constraint WHERE conname = 'fk_intakeformresponses_intakeformresponsestatuses_responsestatusid'
                    ) THEN
                        ALTER TABLE ""IntakeFormResponses"" ADD CONSTRAINT ""FK_IntakeFormResponses_IntakeFormResponseStatuses_ResponseStatusId""
                        FOREIGN KEY (""ResponseStatusId"") REFERENCES ""IntakeFormResponseStatuses""(""Id"") ON DELETE CASCADE;
                    END IF;
                END$$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "IntakeFormResponses");
        }
    }
}
