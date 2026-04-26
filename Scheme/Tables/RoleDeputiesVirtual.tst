<?xml version="1.0" encoding="utf-8"?>
<SchemeTable ID="f2d1426b-e235-432a-bd45-c92a0d7f7874" Name="RoleDeputiesVirtual" Group="Roles" IsVirtual="true" InstanceType="Cards" ContentType="Collections">
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="f2d1426b-e235-002a-2000-092a0d7f7874" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="f2d1426b-e235-012a-4000-092a0d7f7874" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="f2d1426b-e235-002a-3100-092a0d7f7874" Name="RowID" Type="Guid Not Null" />
	<SchemeComplexColumn ID="31c896c4-4583-472c-934d-913bc5ad3e9e" Name="Type" Type="Reference(Typified) Not Null" ReferencedTable="8d6cb6a6-c3f5-4c92-88d7-0cc6b8e8d09d">
		<Description>Тип роли.</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="31c896c4-4583-002c-4000-013bc5ad3e9e" Name="TypeID" Type="Int16 Not Null" ReferencedColumn="c9e1fce6-f27f-4fce-83a0-fadbff72f848">
			<Description>Идентификатор типа роли.</Description>
		</SchemeReferencingColumn>
	</SchemeComplexColumn>
	<SchemeComplexColumn ID="e7ad088b-9f04-4c87-b8f3-db9370917739" Name="Deputy" Type="Reference(Typified) Not Null" ReferencedTable="6c977939-bbfc-456f-a133-f1c2244e3cc3">
		<Description>Персональная роль заместителя.</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="e7ad088b-9f04-0087-4000-0b9370917739" Name="DeputyID" Type="Guid Not Null" ReferencedColumn="6c977939-bbfc-016f-4000-01c2244e3cc3" />
		<SchemeReferencingColumn ID="82aa4a12-e8aa-41ad-b067-e9943aae08ef" Name="DeputyName" Type="String(128) Not Null" ReferencedColumn="1782f76a-4743-4aa4-920c-7edaee860964">
			<Description>Отображаемое имя пользователя.</Description>
		</SchemeReferencingColumn>
	</SchemeComplexColumn>
	<SchemeComplexColumn ID="5287e086-13cd-4a70-b9eb-f649a6843ff3" Name="Deputized" Type="Reference(Typified) Null" ReferencedTable="6c977939-bbfc-456f-a133-f1c2244e3cc3">
		<Description>Персональная роль пользователя, которого замещает пользователь Deputy в этой роли.</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="5287e086-13cd-0070-4000-0649a6843ff3" Name="DeputizedID" Type="Guid Null" ReferencedColumn="6c977939-bbfc-016f-4000-01c2244e3cc3" />
		<SchemeReferencingColumn ID="906a70f1-1687-4fe5-bac8-b7e06d96cf7a" Name="DeputizedName" Type="String(128) Null" ReferencedColumn="1782f76a-4743-4aa4-920c-7edaee860964">
			<Description>Отображаемое имя пользователя.</Description>
		</SchemeReferencingColumn>
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="671985d4-760f-4d32-aacc-61c5f4c53e62" Name="MinDate" Type="Date Not Null">
		<Description>Начальная дата временного замещения или минимальное значение, если замещение постоянное.</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="dd02af44-5245-40b7-9915-2ccb71c6c7f3" Name="MaxDate" Type="Date Not Null">
		<Description>Конечная дата временного замещения или максимальное значение, если замещение постоянное.</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="a285d00f-7b04-4a73-8bd0-96c45d701dc1" Name="IsActive" Type="Boolean Not Null">
		<Description>Признак того, что замещение активно.</Description>
		<SchemeDefaultConstraint IsPermanent="true" ID="bbea684a-65ac-46f6-b01f-4773c83d517e" Name="df_RoleDeputiesVirtual_IsActive" Value="false" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="5a9d627b-e0bc-431f-8da1-3740c08f70f2" Name="IsEnabled" Type="Boolean Not Null">
		<Description>Признак того, что замещение доступно, т.е. может стать активным.</Description>
		<SchemeDefaultConstraint IsPermanent="true" ID="942675c6-4072-4f4d-9400-472a794362a0" Name="df_RoleDeputiesVirtual_IsEnabled" Value="true" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="25641f0b-cf4e-407d-9bb5-2dc10343bc7c" Name="IsPermanent" Type="Boolean Not Null">
		<Description>Признак вечного замещения.</Description>
		<SchemeDefaultConstraint IsPermanent="true" ID="c8915cd8-1b7a-4f2a-9072-1d5b8a62d50e" Name="df_RoleDeputiesVirtual_IsPermanent" Value="false" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="23fb272f-dff2-4859-9d7b-862cdc2095f9" Name="IsDeleteable" Type="Boolean Not Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="c59286fa-fd7f-4656-9b2f-04db49dbc62a" Name="df_RoleDeputiesVirtual_IsDeleteable" Value="true" />
	</SchemePhysicalColumn>
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="f2d1426b-e235-002a-5000-092a0d7f7874" Name="pk_RoleDeputiesVirtual">
		<SchemeIndexedColumn Column="f2d1426b-e235-002a-3100-092a0d7f7874" />
	</SchemePrimaryKey>
	<SchemeIndex IsSystem="true" IsPermanent="true" IsSealed="true" ID="f2d1426b-e235-002a-7000-092a0d7f7874" Name="idx_RoleDeputiesVirtual_ID" IsClustered="true">
		<SchemeIndexedColumn Column="f2d1426b-e235-012a-4000-092a0d7f7874" />
	</SchemeIndex>
</SchemeTable>