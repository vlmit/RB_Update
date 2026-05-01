<?xml version="1.0" encoding="utf-8"?>
<SchemeTable Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="a74ad1f9-b22e-4b28-a914-2a1e27101101" Name="MedoCommonInfo" Group="Custom" InstanceType="Cards" ContentType="Collections">
	<SchemeComplexColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="a74ad1f9-b22e-0028-2000-0a1e27101101" Name="ID" Type="Reference(Typified) Not Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="a74ad1f9-b22e-0128-4000-0a1e27101101" Name="ID" Type="Guid Not Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn IsSystem="true" IsPermanent="true" IsSealed="true" ID="a74ad1f9-b22e-0028-3100-0a1e27101101" Name="RowID" Type="Guid Not Null" />
	<SchemePhysicalColumn ID="0df4c91a-511c-4287-8f66-dc3e4131da9a" Name="MessageID" Type="Guid Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="8743b27d-b679-4bfa-8268-20fb7ac26b7a" Name="df_MedoCommonInfo_MessageID" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="01e2b203-3d80-4fc4-8bb7-093c08680b7e" Name="MedoXsdVersion" Type="Int32 Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="5a9f6cf9-1381-4e8f-9bc4-56613ba34e6e" Name="df_MedoCommonInfo_MedoXsdVersion" Value="1" />
	</SchemePhysicalColumn>
	<SchemeComplexColumn ID="629f6049-1861-4e04-96f0-9695b1273c48" Name="MedoType" Type="Reference(Typified) Null" ReferencedTable="859d4c98-d540-4de7-80a6-d2a2c1bf45bf" WithForeignKey="false">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="6c369aa5-b3e7-41c6-9ec4-c11a612b49d1" Name="MedoTypeID" Type="Int32 Null" ReferencedColumn="707427f5-5221-4af2-b3f1-b5f61846487f">
			<SchemeDefaultConstraint IsPermanent="true" ID="d7c662ab-5497-4eae-8560-c12059ae2dd1" Name="df_MedoCommonInfo_MedoTypeID" Value="8" />
		</SchemeReferencingColumn>
		<SchemeReferencingColumn ID="7128fc43-b00b-4342-a1bf-344f2d0a8704" Name="MedoTypeName" Type="String(128) Null" ReferencedColumn="8ad93980-b156-48b8-a118-4bcfd807c8c5">
			<SchemeDefaultConstraint IsPermanent="true" ID="901baecd-f834-48aa-aa3e-ba03f84cc0fc" Name="df_MedoCommonInfo_MedoTypeName" Value="Документ" />
		</SchemeReferencingColumn>
	</SchemeComplexColumn>
	<SchemeComplexColumn ID="d017c7d3-9953-4054-8d87-57c15664f72c" Name="MedoPartner" Type="Reference(Typified) Null" ReferencedTable="5d47ef13-b6f4-47ef-9815-3b3d0e6d475a">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="d017c7d3-9953-0054-4000-07c15664f72c" Name="MedoPartnerID" Type="Guid Null" ReferencedColumn="5d47ef13-b6f4-01ef-4000-0b3d0e6d475a" />
		<SchemeReferencingColumn ID="7b7d69b8-7026-442c-8f81-601a560c80b1" Name="MedoPartnerName" Type="String(255) Null" ReferencedColumn="f1c960e0-951e-4837-8474-bb61d98f40f0" />
		<SchemeReferencingColumn ID="539f52b0-4d37-4c71-bd61-982ffdf8141e" Name="MedoPartnerFullName" Type="String(450) Null" ReferencedColumn="0e8dd598-19ba-4fa1-9e38-78eb6f9ba074" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="5790dcc7-84d7-4f03-8cf8-3f1daab0a9df" Name="ResponseMesID" Type="Guid Null">
		<SchemeDefaultConstraint IsPermanent="true" ID="33e6e511-ab5d-4c17-ae97-c08d817863e8" Name="df_MedoCommonInfo_ResponseMesID" />
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="e4d445ed-8d93-4d5c-9e73-cfe058cf69e3" Name="DateMessage" Type="DateTime Null">
		<Description>дата сообщения</Description>
	</SchemePhysicalColumn>
	<SchemeComplexColumn ID="b124d968-1de7-4a58-a913-bb89690ece8b" Name="MedoStatus" Type="Reference(Typified) Null" ReferencedTable="3b57fbdb-73a2-49d9-8e80-5a73b5af9ad7" WithForeignKey="false">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="8d06d940-fd6d-4542-8ed2-4aab66af2043" Name="MedoStatusID" Type="Int32 Null" ReferencedColumn="05be4306-54fe-435b-ae00-cedc2a93e62b">
			<SchemeDefaultConstraint IsPermanent="true" ID="123496ee-dbbd-4d6f-800b-e302b511cd8a" Name="df_MedoCommonInfo_MedoStatusID" Value="0" />
		</SchemeReferencingColumn>
		<SchemeReferencingColumn ID="1ef06593-71ff-40a1-837c-597a658dabef" Name="MedoStatusName" Type="String(128) Null" ReferencedColumn="43111178-66e7-4e34-8a65-904a8921a8d7">
			<SchemeDefaultConstraint IsPermanent="true" ID="f1eda567-b9e4-42d5-b2b2-76bc08b4a599" Name="df_MedoCommonInfo_MedoStatusName" Value="Отправляется" />
		</SchemeReferencingColumn>
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="06e9fbea-2455-4034-9fcc-95a4c6f5a3aa" Name="MedoComment" Type="String(Max) Null" />
	<SchemePhysicalColumn ID="4ece0f0d-9b40-4d6d-a546-dacdf988b53b" Name="MedoError" Type="String(Max) Null">
		<Description>тест</Description>
	</SchemePhysicalColumn>
	<SchemeComplexColumn ID="e6950f68-43b3-41ac-b55c-c4ab3539b9ac" Name="Person" Type="Reference(Typified) Null" ReferencedTable="6c977939-bbfc-456f-a133-f1c2244e3cc3">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="e6950f68-43b3-00ac-4000-04ab3539b9ac" Name="PersonID" Type="Guid Null" ReferencedColumn="6c977939-bbfc-016f-4000-01c2244e3cc3" />
		<SchemeReferencingColumn ID="f99d8aea-4f14-461b-948b-d98448356b38" Name="PersonFullName" Type="String(256) Null" ReferencedColumn="e89b6dc3-7932-4d74-a99f-91b402029536" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn ID="dde5a6a9-ab19-4701-8a8f-3898ef014509" Name="SendDate" Type="DateTime Null">
		<Description>Дата отправки при завершении задачи</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn ID="8dc45a03-2a94-4915-9752-027eab663af6" Name="SedRowID" Type="Guid Null" />
	<SchemePrimaryKey IsSystem="true" IsPermanent="true" IsSealed="true" ID="a74ad1f9-b22e-0028-5000-0a1e27101101" Name="pk_MedoCommonInfo">
		<SchemeIndexedColumn Column="a74ad1f9-b22e-0028-3100-0a1e27101101" />
	</SchemePrimaryKey>
	<SchemeIndex IsSystem="true" IsPermanent="true" IsSealed="true" ID="a74ad1f9-b22e-0028-7000-0a1e27101101" Name="idx_MedoCommonInfo_ID" IsClustered="true">
		<SchemeIndexedColumn Column="a74ad1f9-b22e-0128-4000-0a1e27101101" />
	</SchemeIndex>
</SchemeTable>