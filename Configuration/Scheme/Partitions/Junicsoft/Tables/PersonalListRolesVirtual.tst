<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="e1b3cb70-f7fc-4b62-b822-e5302d673356" ID="7a2dfe5c-b049-421c-b67f-93ad53355485" Name="PersonalListRolesVirtual" Group="Junicsoft" IsVirtual="true" InstanceType="Cards" ContentType="Collections">
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="7a2dfe5c-b049-001c-2000-03ad53355485" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="7a2dfe5c-b049-011c-4000-03ad53355485" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="7a2dfe5c-b049-001c-3100-03ad53355485" Name="RowID" Type="Guid Not Null" />
	<SchemeComplexColumn ID="979650cc-be10-4be3-a9fc-88e15a7aaf33" Name="Role" Type="Reference(Typified) Null" ReferencedTable="6c977939-bbfc-456f-a133-f1c2244e3cc3">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="979650cc-be10-00e3-4000-08e15a7aaf33" Name="RoleID" Type="Guid Null" ReferencedColumn="6c977939-bbfc-016f-4000-01c2244e3cc3" />
		<SchemeReferencingColumn ID="7176c035-83a1-4a19-8160-7b5dfd4f26e0" Name="RoleName" Type="String(128) Null" ReferencedColumn="1782f76a-4743-4aa4-920c-7edaee860964" />
	</SchemeComplexColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="7a2dfe5c-b049-001c-5000-03ad53355485" Name="pk_PersonalListRolesVirtual">
		<SchemeIndexedColumn Column="7a2dfe5c-b049-001c-3100-03ad53355485" />
	</SchemePrimaryKey>
	<SchemeIndex IsSystem="true" IsPermanent="true" IsSealed="true" ID="7a2dfe5c-b049-001c-7000-03ad53355485" Name="idx_PersonalListRolesVirtual_ID" IsClustered="true">
		<SchemeIndexedColumn Column="7a2dfe5c-b049-011c-4000-03ad53355485" />
	</SchemeIndex>
</SchemeTable>