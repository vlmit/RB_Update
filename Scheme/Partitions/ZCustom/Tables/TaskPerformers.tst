<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="46ed9142-6079-477b-afab-d1930716751e" Name="TaskPerformers" Group="Custom" InstanceType="Tasks" ContentType="Collections">
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="46ed9142-6079-007b-2000-01930716751e" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="5bfa9936-bb5a-4e8f-89a9-180bfd8f75f8">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="46ed9142-6079-017b-4000-01930716751e" Name="ID" Type="Guid Not Null" ReferencedColumn="5bfa9936-bb5a-008f-3100-080bfd8f75f8" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="46ed9142-6079-007b-3100-01930716751e" Name="RowID" Type="Guid Not Null" />
	<SchemeComplexColumn ID="9bffe3c8-ae41-4fa2-b1d9-757a3881cc11" Name="TaskPerformer" Type="Reference(Typified) Null" ReferencedTable="6c977939-bbfc-456f-a133-f1c2244e3cc3" WithForeignKey="false">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="9bffe3c8-ae41-00a2-4000-057a3881cc11" Name="TaskPerformerID" Type="Guid Null" ReferencedColumn="6c977939-bbfc-016f-4000-01c2244e3cc3" />
		<SchemeReferencingColumn ID="1ccfda9f-43b9-4c19-8204-db38d68caade" Name="TaskPerformerName" Type="String(128) Null" ReferencedColumn="1782f76a-4743-4aa4-920c-7edaee860964" />
	</SchemeComplexColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="46ed9142-6079-007b-5000-01930716751e" Name="pk_TaskPerformers">
		<SchemeIndexedColumn Column="46ed9142-6079-007b-3100-01930716751e" />
	</SchemePrimaryKey>
	<SchemeIndex IsSystem="true" IsPermanent="true" IsSealed="true" ID="46ed9142-6079-007b-7000-01930716751e" Name="idx_TaskPerformers_ID" IsClustered="true">
		<SchemeIndexedColumn Column="46ed9142-6079-017b-4000-01930716751e" />
	</SchemeIndex>
</SchemeTable>