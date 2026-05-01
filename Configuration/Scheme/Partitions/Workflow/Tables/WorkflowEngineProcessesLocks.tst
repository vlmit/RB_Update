<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="dd8eeaba-9042-4fb5-9e8e-f7544463464f" ID="bb0d570d-2213-49b8-bea6-89b5b63a602f" Name="WorkflowEngineProcessesLocks" Group="WorkflowEngine" InstanceType="Cards" ContentType="Collections">
	<Description>Таблица с состоянием блокировок экземпляров процессов</Description>
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="bb0d570d-2213-00b8-2000-09b5b63a602f" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="bb0d570d-2213-01b8-4000-09b5b63a602f" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="bb0d570d-2213-00b8-3100-09b5b63a602f" Name="RowID" Type="Guid Not Null" />
	<SchemePhysicalColumn ID="7457682d-0888-4281-83cc-d3bba2d97fcb" Name="Locked" Type="DateTime Null">
		<Description>Определяет дату/время взятия блокировки на процесс</Description>
	</SchemePhysicalColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="bb0d570d-2213-00b8-5000-09b5b63a602f" Name="pk_WorkflowEngineProcessesLocks" IsClustered="true">
		<SchemeIndexedColumn Column="bb0d570d-2213-00b8-3100-09b5b63a602f" />
	</SchemePrimaryKey>
	<SchemeIndex IsSystem="true" IsPermanent="true" IsSealed="true" ID="bb0d570d-2213-00b8-7000-09b5b63a602f" Name="idx_WorkflowEngineProcessesLocks_ID">
		<SchemeIndexedColumn Column="bb0d570d-2213-01b8-4000-09b5b63a602f" />
	</SchemeIndex>
</SchemeTable>