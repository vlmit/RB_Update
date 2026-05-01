<?xml version="1.0" encoding="utf-8"?>
<SchemeTable ID="a161e289-2f99-4699-9e95-6e3336be8527" Partition="d1b372f3-7565-4309-9037-5e5a0969d94e">
	<SchemeComplexColumn ID="36be79af-fff7-417e-997e-4fe3f51aae56" Name="Department">
		<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="f805c74a-24b3-44b4-9141-7b5f6ac3fde1" Name="DepartmentIndexDistrict" Type="String(128) Null" />
		<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="f1c12699-f648-466d-8907-d98c6e9c051d" Name="DepartmentIndexDep" Type="String(128) Null" />
	</SchemeComplexColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="6637c3dd-eef2-48e2-8bda-2460f3095a2e" Name="Nomenclature" Type="Reference(Typified) Null" ReferencedTable="27454eac-5316-4d34-baa6-31e0379447f6" WithForeignKey="false">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="6637c3dd-eef2-00e2-4000-0460f3095a2e" Name="NomenclatureID" Type="Guid Null" ReferencedColumn="27454eac-5316-0134-4000-01e0379447f6" />
		<SchemeReferencingColumn ID="96e72bcd-98b1-48b6-8bd1-1806890fca2a" Name="NomenclatureIndex" Type="String(128) Null" ReferencedColumn="ecbfd5ce-7268-4bfc-98ec-d903d80124a6" />
		<SchemeReferencingColumn ID="cb1706cb-eae7-4faf-ac01-f0831f20eec5" Name="NomenclatureDescription" Type="String(512) Null" ReferencedColumn="db1d439d-2e90-4233-8127-b2cdc6a027cf" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="01c74af2-24f0-455c-b9a7-25d2fdb601b6" Name="Contents" Type="String(256) Null">
		<Description>Содержание</Description>
	</SchemePhysicalColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="d4d6e703-4007-4396-81ce-4ac65b129184" Name="DeliveryType" Type="Reference(Typified) Null" ReferencedTable="366fa575-2ada-446b-afa1-a68eca0ab8db">
		<Description>Тип доставки</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="d4d6e703-4007-0096-4000-0ac65b129184" Name="DeliveryTypeID" Type="Guid Null" ReferencedColumn="366fa575-2ada-016b-4000-068eca0ab8db" />
		<SchemeReferencingColumn ID="dbc7b3f5-6e3a-4649-a7aa-f377c972ecb9" Name="DeliveryTypeName" Type="String(128) Null" ReferencedColumn="d1d10a19-8cb9-42f2-b242-69ae0e8f2ecd" />
	</SchemeComplexColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="85fe5d50-b460-4b09-acf9-d1a2e50d9841" Name="DocTypeMEDO" Type="Reference(Typified) Null" ReferencedTable="06684526-4757-45fa-a9b8-bd1598c22d09">
		<Description>Вид документа МЭДО</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="85fe5d50-b460-0009-4000-01a2e50d9841" Name="DocTypeMEDOID" Type="Guid Null" ReferencedColumn="06684526-4757-01fa-4000-0d1598c22d09" />
		<SchemeReferencingColumn ID="706d62ab-7cec-4dd5-bc22-74ecd371e6bb" Name="DocTypeMEDOName" Type="String(256) Null" ReferencedColumn="5297e59d-0b65-4aa6-a896-78842779205a" />
		<SchemeReferencingColumn ID="8d4f34ed-b59d-4e35-9214-bb36277d82fd" Name="DocTypeMEDOIndex" Type="String(128) Null" ReferencedColumn="d2459c96-fef7-45ba-89f4-50e36b6f1fd1" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="19314671-9dd7-4729-bf46-d1a83163bc17" Name="Comment" Type="String(1024) Null">
		<Description>Комментарий</Description>
	</SchemePhysicalColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="a8750aee-c99e-471c-874e-51e5f9c30ff3" Name="DepartmentRecipient" Type="Reference(Typified) Null" ReferencedTable="d43dace1-536f-4c9f-af15-49a8892a7427">
		<Description>Адресат_подразделение</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="a8750aee-c99e-001c-4000-01e5f9c30ff3" Name="DepartmentRecipientID" Type="Guid Null" ReferencedColumn="d43dace1-536f-019f-4000-09a8892a7427" />
		<SchemePhysicalColumn ID="820fc124-e6b7-4c56-95ba-b99e98d1b9be" Name="DepartmentRecipientName" Type="String(128) Null" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="48169aaf-b87f-4c03-9266-35fafb503e10" Name="ExternalGuid" Type="String(128) Null">
		<Description>Внешний Guid</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="d476db4c-cb28-4e13-b18c-4866fbfdcf68" Name="LastUploadOut" Type="DateTime Null">
		<Description>Время последней выгрузки</Description>
	</SchemePhysicalColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="4ac9a349-4ec1-433f-92b8-fb38228fec46" Name="TaskTextTemplate" Type="Reference(Typified) Null" ReferencedTable="4a5d1567-6e66-4057-83b3-c9eeb3248f42">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="4ac9a349-4ec1-003f-4000-0b38228fec46" Name="TaskTextTemplateID" Type="Guid Null" ReferencedColumn="4a5d1567-6e66-0157-4000-09eeb3248f42" />
		<SchemeReferencingColumn ID="4d029b03-e3ba-46c1-875b-4a15fd7afa85" Name="TaskTextTemplateName" Type="String(Max) Null" ReferencedColumn="261dc8e1-46f9-4863-9adc-6f42ed8e1c94" />
		<SchemePhysicalColumn ID="2ebb740d-05c5-4f68-b633-280ac0314b95" Name="TaskTextTemplateManual" Type="String(1024) Null">
			<Description>Текст введенный вручную</Description>
		</SchemePhysicalColumn>
	</SchemeComplexColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="94564480-43b4-4331-a7c1-79f4837e21ad" Name="TaskDeadline" Type="Date Null">
		<Description>Срок поручения</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="35960925-5c34-4d0b-bdba-0284c87f867f" Name="TaskFact" Type="Date Null">
		<Description>Фактическая дата исполнения поручения</Description>
	</SchemePhysicalColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="2a35ec16-5aca-4559-948e-1422b8ba6007" Name="Parent" Type="Reference(Typified) Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e" WithForeignKey="false">
		<Description>Документ-основание</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="2a35ec16-5aca-0059-4000-0422b8ba6007" Name="ParentID" Type="Guid Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
		<SchemePhysicalColumn ID="80e36cad-b4fd-4da4-ab2b-5cf4b400ae89" Name="ParentDescription" Type="String(256) Null" />
	</SchemeComplexColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="6d315be8-15bd-4bb6-99fe-a07755045a45" Name="ParentTask" Type="Reference(Typified) Null" ReferencedTable="1074eadd-21d7-4925-98c8-40d1e5f0ca0e" WithForeignKey="false">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="6d315be8-15bd-00b6-4000-007755045a45" Name="ParentTaskID" Type="Guid Null" ReferencedColumn="9a58123b-b2e9-4137-9c6c-5dab0ec02747" />
		<SchemePhysicalColumn ID="ea83408f-65c8-4d80-a279-c383d0b307e6" Name="ParentTaskDescription" Type="String(256) Null" />
	</SchemeComplexColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="90f729b6-7fb0-41d6-a974-4b3d387bbcd4" Name="ManualState" Type="Reference(Typified) Null" ReferencedTable="47107d7a-3a8c-47f0-b800-2a45da222ff4">
		<Description>Состояние по документу, измененное вручную</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="90f729b6-7fb0-00d6-4000-0b3d387bbcd4" Name="ManualStateID" Type="Int16 Null" ReferencedColumn="502209b0-233f-4e1f-be01-35a50f53414c" />
		<SchemeReferencingColumn ID="91fab7ce-e8fc-44ab-a163-acb29accddf4" Name="ManualStateName" Type="String(128) Null" ReferencedColumn="4c1a8dd7-72ed-4fc9-b559-b38ae30dccb9" />
	</SchemeComplexColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="92ac4a17-b1f6-4469-9b47-db2eadfd6141" Name="GroundsDeregistration" Type="Reference(Typified) Null" ReferencedTable="b7e096ef-ff8d-4f9c-9c19-d27c2dc9758e">
		<Description>Основание для снятия с контроля</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="92ac4a17-b1f6-0069-4000-0b2eadfd6141" Name="GroundsDeregistrationID" Type="Int16 Null" ReferencedColumn="52039ceb-78ce-4ee5-9ef0-1d1ea3bd866f" />
		<SchemeReferencingColumn ID="f57ec7b2-9525-45d4-93a4-44a3a09faecf" Name="GroundsDeregistrationName" Type="String(128) Null" ReferencedColumn="4a7ca97b-2f68-4e2d-937a-af09ec77c1fa" />
	</SchemeComplexColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="5dc843d8-4c22-43ba-b413-88a3c5fdf000" Name="ControlStatus" Type="Reference(Typified) Null" ReferencedTable="e7df3509-f7f2-4c6e-862d-5893f30e8426">
		<Description>Статус контроля</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="5dc843d8-4c22-00ba-4000-08a3c5fdf000" Name="ControlStatusID" Type="Int16 Null" ReferencedColumn="c251147f-9ae7-4c7c-b634-12c1f04a2877" />
		<SchemeReferencingColumn ID="ceb96207-0711-426e-9061-b8481418fa94" Name="ControlStatusName" Type="String(128) Null" ReferencedColumn="30f451ea-6009-49ea-b427-61727a926e64" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="eb0fc292-cd02-4111-9216-cb5a8ef967ef" Name="PlanDate" Type="Date Null">
		<Description>Плановая дата</Description>
	</SchemePhysicalColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="f73a3714-bf23-40d2-9669-f39d462be697" Name="Controller" Type="Reference(Typified) Null" ReferencedTable="6c977939-bbfc-456f-a133-f1c2244e3cc3">
		<Description>Контролёр</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="f73a3714-bf23-00d2-4000-039d462be697" Name="ControllerID" Type="Guid Null" ReferencedColumn="6c977939-bbfc-016f-4000-01c2244e3cc3" />
		<SchemeReferencingColumn ID="390cb3e0-e77e-4418-b109-b87c211e1696" Name="ControllerName" Type="String(128) Null" ReferencedColumn="1782f76a-4743-4aa4-920c-7edaee860964" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="646286d5-0b1b-4cec-91f2-5a1852ec5c5c" Name="OutgoingDate" Type="Date Null">
		<Description>Дата внеш.</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="a062fd9b-0bcc-4cef-813b-7a3286ce9749" Name="OrderPoint" Type="String(128) Null">
		<Description>Пункт поручения</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="f22b7f6d-670d-40ed-8e86-d9757cff0e60" Name="NumberOfTransfers" Type="Int16 Null">
		<Description>Количество переносов</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="d40128a1-1205-4d8d-b413-29de3a45b7b4" Name="ExpandTextTask" Type="String(Max) Null">
		<Description>Содержание поручения</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="fa634d78-8939-4004-ab53-aabdef85cfa4" Name="CurrentStateSP" Type="String(128) Null">
		<Description>Статус полученный из SP</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="144347f7-7ab8-437a-83a9-0c3dc0a7e4a2" Name="IsMigration" Type="Boolean Null">
		<Description>Признак что документ получен при помощи миграции</Description>
	</SchemePhysicalColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="1c77e234-896f-4092-9f5f-7c5522cbf47c" Name="Original" Type="Reference(Typified) Null" ReferencedTable="1361e4ec-a290-4e34-a36a-d82f3ba9d120">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="1c77e234-896f-0092-4000-0c5522cbf47c" Name="OriginalID" Type="Int16 Null" ReferencedColumn="e46195f7-bf3c-448f-9f89-3b31fdf6a9a1" />
		<SchemeReferencingColumn ID="24809105-2880-482a-9bfd-c21b878b3167" Name="OriginalName" Type="String(128) Null" ReferencedColumn="522b3c36-9509-4490-86a3-1c7d1a27e396" />
	</SchemeComplexColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="cb550ec4-b5e9-4797-bbab-e2eb2f5ce5c8" Name="assignedTo" Type="Reference(Typified) Null" ReferencedTable="6c977939-bbfc-456f-a133-f1c2244e3cc3" WithForeignKey="false">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="cb550ec4-b5e9-0097-4000-02eb2f5ce5c8" Name="assignedToID" Type="Guid Null" ReferencedColumn="6c977939-bbfc-016f-4000-01c2244e3cc3" />
		<SchemeReferencingColumn ID="13462c1f-6627-4723-8828-ca88fda8e76b" Name="assignedToName" Type="String(128) Null" ReferencedColumn="1782f76a-4743-4aa4-920c-7edaee860964" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="8cd7d6cd-92eb-4c8f-8d31-8e94d4c88f51" Name="Notes" Type="String(Max) Null" />
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="d4f47ac0-bb83-45b7-8d81-88e72a32769b" Name="ExternalGuidForTask" Type="String(128) Null">
		<Description>Внешний Guid для поручения</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="ca86a1dc-0094-4cae-8059-b9d9ed6a0a22" Name="LastDepartmentSend" Type="String(128) Null">
		<Description>Запись крайнего Подразделения при интеграции</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="2c02e4db-b286-498b-947a-36d9306e4d08" Name="Signed" Type="String(Max) Null">
		<Description>поле Подписал</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="c96e293f-6d71-4d60-ac0e-b3b27667525e" Name="ShortContent" Type="String(Max) Null">
		<Description>Краткое содержание</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="c8022b7a-39df-4078-9605-1ffec465653d" Name="TaskFactControl" Type="Date Null">
		<Description>Дата снятия с контроля</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="a0a67a18-426c-4b36-9d5b-e4a4405c6712" Name="NotesForHead" Type="String(Max) Null">
		<Description>Комментарий об исполнении поручения (для руководителей)</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="af3f40cc-dd52-413c-95e6-33f17fa8c2fa" Name="TaskDeadlineReserv" Type="Date Null" />
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="11b47818-24df-44c6-85d5-6400632d99ea" Name="FinishDate" Type="Date Null">
		<Description>Дата завершения</Description>
	</SchemePhysicalColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="31ac7f40-8a91-40e8-ad09-5800889870a2" Name="CommissionControl" Type="Reference(Typified) Null" ReferencedTable="cc017f2f-e99d-4e89-be52-5e9ed5056c91" WithForeignKey="false">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="31ac7f40-8a91-00e8-4000-0800889870a2" Name="CommissionControlID" Type="Int16 Null" ReferencedColumn="c62d365b-f051-4a3d-a8b7-2d02f8e477bb" />
		<SchemeReferencingColumn ID="35f1118f-465a-43e2-91a1-785bb492d1fc" Name="CommissionControlName" Type="String(128) Null" ReferencedColumn="b04d4c60-ca70-4eb5-b836-3fb1e2dfcf30" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="554c5387-3454-4c8c-9d9c-95acaffeb929" Name="TreeLevel" Type="String(128) Null">
		<Description>SP Migration</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="ecb99dde-8fa3-4f19-8a84-aa21fcf82933" Name="TreeIndex" Type="String(256) Null" />
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="7cc6518e-601a-4e57-bc6e-e69d4f1610fe" Name="ParentTreeIndex" Type="String(256) Null" />
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="e0353a44-7b14-493a-82e6-d9c39ac8601a" Name="MedoDocTypeCode " Type="String(1024) Null" />
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="26964364-948b-44b1-9502-4eb453ac035a" Name="MedoDocTypeName" Type="String(256) Null" />
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="6879538c-9315-4877-be06-71a90c10f7ef" Name="SheetsCount" Type="Int32 Null">
		<Description>Количество страниц</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="d39c33e9-9c71-4baa-872d-ecdafbc361c6" Name="PageNumber" Type="Int32 Null" />
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="509fdd63-f99b-449f-8d9b-e2f09273dfea" Name="SignedT" Type="String(256) Null" />
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="0712587b-a63a-4857-879f-791851ea7296" Name="RefusalReason" Type="Reference(Typified) Null" ReferencedTable="2b80b126-863e-4079-80c4-2e12a6d79a82">
		<Description>Причины отказа в регистрации</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="0712587b-a63a-0057-4000-091851ea7296" Name="RefusalReasonID" Type="Guid Null" ReferencedColumn="2b80b126-863e-0179-4000-0e12a6d79a82" />
		<SchemeReferencingColumn ID="beb44120-5590-4b28-9099-610da23552b9" Name="RefusalReasonName" Type="String(Max) Null" ReferencedColumn="0c12a991-c4c5-4139-a37b-3e31ed6a5b34" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="54bba29f-7ac7-4ee3-bc60-93a129dea9cc" Name="IsControl" Type="Boolean Null">
		<Description>Создан ли документ на основании поручения</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="b894cc2a-ce29-4293-a4f7-713ab51bd3f5" Name="ControlID" Type="Guid Null">
		<Description>ID карточки поручения</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="1c5d27e5-2b6f-4df5-94f0-6259741da2fb" Name="ControlRowID" Type="Guid Null">
		<Description>ID строки в таблице поручения </Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="0d073c3b-e726-4e73-b4ae-3a008a9d048b" Name="PerformersWithoutSED" Type="String(Max) Null">
		<Description>Исполнители</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="b129d857-4709-4ea4-95d4-bb34c2af8b52" Name="DivisionWithoutSED" Type="String(Max) Null">
		<Description>Подразделение</Description>
	</SchemePhysicalColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="14e52198-07be-468a-a9be-e0867471e3f3" Name="NPAType" Type="Reference(Typified) Null" ReferencedTable="4bf059f1-0fef-4789-886e-4ac5ec01ffb3">
		<Description>Вид НПА</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="14e52198-07be-008a-4000-00867471e3f3" Name="NPATypeID" Type="Guid Null" ReferencedColumn="4bf059f1-0fef-0189-4000-0ac5ec01ffb3" />
		<SchemeReferencingColumn ID="d286fa72-f293-41b6-b2b6-3741a10f1cd6" Name="NPATypeName" Type="String(Max) Null" ReferencedColumn="d1408cd4-2992-4121-8b8e-a3852e297d87" />
		<SchemeReferencingColumn ID="ae49702d-f935-40bf-a3cd-e4ea8b687dc8" Name="NPATypeIndex" Type="String(Max) Null" ReferencedColumn="6d789ed3-4b27-4cd0-a01d-26a431d457be" />
	</SchemeComplexColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="cbfca00f-0091-4029-86e2-f84c3a358292" Name="AcceptanceStatus" Type="Reference(Typified) Null" ReferencedTable="6ef0270f-dbdf-40bb-be8c-74b21c376fdf" WithForeignKey="false">
		<Description>Cтатус принятия</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="cbfca00f-0091-0029-4000-084c3a358292" Name="AcceptanceStatusID" Type="Guid Null" ReferencedColumn="6ef0270f-dbdf-01bb-4000-04b21c376fdf" />
		<SchemeReferencingColumn ID="defe1547-c66e-489f-a80f-3608f2be2daf" Name="AcceptanceStatusName" Type="String(Max) Null" ReferencedColumn="c61a2a8b-495d-4512-807e-9a7fa3dd2bb8" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="b27f7979-6c28-4060-8ee6-f0a17b9286b8" Name="NumNPA" Type="String(Max) Null">
		<Description>№ НПА</Description>
	</SchemePhysicalColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="c494267b-28ba-4bf4-993a-426a2c63403c" Name="CommissionTemplate" Type="Reference(Typified) Null" ReferencedTable="67febfaf-f5f5-4b3d-9363-8a5053c38d79" WithForeignKey="false">
		<Description>Текст поручения вручную или из шаблона</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="c494267b-28ba-00f4-4000-026a2c63403c" Name="CommissionTemplateID" Type="Guid Null" ReferencedColumn="67febfaf-f5f5-013d-4000-0a5053c38d79" />
		<SchemeReferencingColumn ID="1d65c792-f1be-400d-b0a4-7be7b4aad466" Name="CommissionTemplateText" Type="String(Max) Null" ReferencedColumn="3686b71d-5441-4cbc-a2c8-c19eebb66f3f" />
	</SchemeComplexColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="524c8487-5b5f-47a1-a485-0033645995f9" Name="TypeOfDocument" Type="Reference(Typified) Null" ReferencedTable="6000e7dd-0ef7-4a4e-b6a8-f01d8545b15b">
		<Description>Вид документа</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="524c8487-5b5f-00a1-4000-0033645995f9" Name="TypeOfDocumentID" Type="Guid Null" ReferencedColumn="6000e7dd-0ef7-014e-4000-001d8545b15b" />
		<SchemeReferencingColumn ID="a8d6f48d-e223-4f04-b8b6-d16eb112f184" Name="TypeOfDocumentName" Type="String(Max) Null" ReferencedColumn="34b4147b-9834-4190-ba6e-ae8b5b838d36" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="049ac523-4892-4844-8d4d-a2229711f89e" Name="Title" Type="String(Max) Null" />
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="da6211f2-43e9-48e7-aa10-46715426ed98" Name="Urgency" Type="Reference(Typified) Null" ReferencedTable="d9c38557-c1c1-441b-8699-e4e3e0619ee3">
		<Description>Срочност</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="da6211f2-43e9-00e7-4000-06715426ed98" Name="UrgencyID" Type="Guid Null" ReferencedColumn="d9c38557-c1c1-011b-4000-04e3e0619ee3">
			<SchemeDefaultConstraint IsPermanent="true" ID="cd2b4ddf-8e68-4f87-9a22-3daf878399a5" Name="df_DocumentCommonInfo_UrgencyID" />
		</SchemeReferencingColumn>
		<SchemeReferencingColumn ID="a07b2025-4a2c-42af-a6c2-61b684158931" Name="UrgencyName" Type="String(Max) Null" ReferencedColumn="22caea1f-324a-43aa-99b3-e7f8c54c1994">
			<SchemeDefaultConstraint IsPermanent="true" ID="589870ad-d796-482c-8101-61c11b603d1a" Name="df_DocumentCommonInfo_UrgencyName" />
		</SchemeReferencingColumn>
	</SchemeComplexColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="f4149d9e-21c5-4efb-b317-6a6e7cc91295" Name="SheetsAttachmentsCount" Type="Int32 Null">
		<Description>Количество страниц приложений</Description>
	</SchemePhysicalColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="340bbb62-0c79-4ff3-8a61-a0f0f067466f" Name="DepAGIP" Type="Reference(Typified) Null" ReferencedTable="d43dace1-536f-4c9f-af15-49a8892a7427">
		<Description>Подразделение (АГиП)</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="340bbb62-0c79-00f3-4000-00f0f067466f" Name="DepAGIPID" Type="Guid Null" ReferencedColumn="d43dace1-536f-019f-4000-09a8892a7427" />
		<SchemePhysicalColumn ID="9d94c5a4-ed1b-4d83-898f-070387979b56" Name="DepAGIPName" Type="String(128) Null" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="ba9117f3-89c2-4966-a327-979de2a78d6b" Name="WaitinigTaskID" Type="Guid Null">
		<Description></Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="755f4921-bc40-4933-9bbb-c1d0719f9e28" Name="MEDOFolderName" Type="String(Max) Null">
		<Description>Столбец с названием папки для входящих МЭДО</Description>
	</SchemePhysicalColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="298f8c78-9c45-4a72-a056-47e7e0598409" Name="Responsible" Type="Reference(Typified) Null" ReferencedTable="6c977939-bbfc-456f-a133-f1c2244e3cc3">
		<Description>Ответственный</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="298f8c78-9c45-0072-4000-07e7e0598409" Name="ResponsibleID" Type="Guid Null" ReferencedColumn="6c977939-bbfc-016f-4000-01c2244e3cc3" />
		<SchemeReferencingColumn ID="8a55ed5e-4438-4ffd-847d-473db674e4d1" Name="ResponsibleName" Type="String(128) Null" ReferencedColumn="1782f76a-4743-4aa4-920c-7edaee860964" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="bc234b36-3961-452e-9b66-d94e5043b600" Name="LastRegIncMedoID" Type="Guid Null">
		<Description>Поле для хранения зарегистрированного входящегоМЭДО</Description>
	</SchemePhysicalColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="8c2876d4-e2b1-4d0e-8347-f9f9e74612d2" Name="UrgencyNPA" Type="Reference(Typified) Null" ReferencedTable="d9c38557-c1c1-441b-8699-e4e3e0619ee3">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="8c2876d4-e2b1-000e-4000-09f9e74612d2" Name="UrgencyNPAID" Type="Guid Null" ReferencedColumn="d9c38557-c1c1-011b-4000-04e3e0619ee3" />
		<SchemeReferencingColumn ID="26a6b266-272a-4cd3-89ac-9ea3995f3738" Name="UrgencyNPAName" Type="String(Max) Null" ReferencedColumn="22caea1f-324a-43aa-99b3-e7f8c54c1994" />
	</SchemeComplexColumn>
	<SchemeComplexColumn ID="22ed59ec-4939-4d96-a7a8-cbf0bda55ec0" Name="Partner">
		<SchemeReferencingColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="39e6b05b-6f47-4264-b178-c461b25af088" Name="PartnerMedoID" Type="String(Max) Null" ReferencedColumn="745788ea-1a23-472c-9eb0-c48d2dfea546" />
	</SchemeComplexColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="73e5211c-fd18-4052-8478-ce9eef9321a7" Name="Executor" Type="Reference(Typified) Null" ReferencedTable="6c977939-bbfc-456f-a133-f1c2244e3cc3" WithForeignKey="false">
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="73e5211c-fd18-0052-4000-0e9eef9321a7" Name="ExecutorID" Type="Guid Null" ReferencedColumn="6c977939-bbfc-016f-4000-01c2244e3cc3" />
		<SchemeReferencingColumn ID="94d2bee4-022c-4749-99ac-3f1b2dc9dc1f" Name="ExecutorName" Type="String(128) Null" ReferencedColumn="1782f76a-4743-4aa4-920c-7edaee860964" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="87cfbab1-1d02-4463-a5a1-01329e53097e" Name="PowerOfAttorney" Type="String(Max) Null">
		<Description>Доверенность</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="52939ba9-abf6-4c3a-98ca-0d15a3639aed" Name="DateStartTreaty" Type="Date Null">
		<Description>Дата начала договора</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="6b7d79f9-a177-4de3-b44b-7450b0f59b14" Name="DateEndTreaty" Type="Date Null">
		<Description>Дата окончания договора</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="4dcd0bca-fe71-4acb-ba78-0c2456923c30" Name="DateFactTreaty" Type="Date Null">
		<Description>Фатическая дата договора</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="5cb1f4e1-a552-4704-9edc-55f7e7eb7dad" Name="NDS" Type="Decimal(18, 2) Null">
		<Description>ДНС</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="b47e7299-0256-495b-8eae-21957ba2be63" Name="AmountNDS" Type="Decimal(18, 2) Null">
		<Description>Сумма, облагаемая ДНС</Description>
	</SchemePhysicalColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="c1520601-99f8-4992-9863-257ff037ef63" Name="SumNDS" Type="Decimal(18, 2) Null">
		<Description>Сумма с ДНС</Description>
	</SchemePhysicalColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="4f4d1ba0-bfbf-4cc2-a251-0da869be7fa1" Name="NDSRate" Type="Reference(Typified) Null" ReferencedTable="b7404c8f-eef1-48ec-a750-cbcbeb8be283">
		<Description>Ставка ДНС</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="4f4d1ba0-bfbf-00c2-4000-0da869be7fa1" Name="NDSRateID" Type="Guid Null" ReferencedColumn="b7404c8f-eef1-01ec-4000-0bcbeb8be283" />
		<SchemeReferencingColumn ID="28f3cfe8-5494-4e32-949d-a131c3698789" Name="NDSRateRate" Type="String(Max) Null" ReferencedColumn="ea691237-6500-4f85-b357-af3018781279" />
	</SchemeComplexColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="10107298-69c8-457c-8498-18dd60dbdfdf" Name="MEDORejReason" Type="Reference(Typified) Null" ReferencedTable="89c2e318-a1c4-4a25-99be-c2be0ac70b7c">
		<SchemeReferencingColumn ID="4497ab9f-aff6-4491-90fe-b1e1381d5973" Name="MEDORejReasonID" Type="Int16 Null" ReferencedColumn="ed43c393-8844-42e8-9a6f-0959f83c6b90" />
	</SchemeComplexColumn>
	<SchemePhysicalColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="7da20e5a-f2a9-4dee-a2cb-b36999aec369" Name="MEDOMessageUID" Type="String(Max) Null" />
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="da04653f-709c-435a-aea0-33a5760a9241" Name="ReasonsForCreation" Type="Reference(Typified) Null" ReferencedTable="8e00fba3-f00a-4755-9689-a28d56e27999">
		<Description>Причины создания</Description>
		<SchemeReferencingColumn ID="248debab-1ae9-45bb-aa3a-935110e6c37d" Name="ReasonsForCreationName" Type="String(Max) Null" ReferencedColumn="190d6c43-8194-4a53-bc3d-a0b6afcd374a" />
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="da04653f-709c-005a-4000-03a5760a9241" Name="ReasonsForCreationID" Type="Guid Null" ReferencedColumn="8e00fba3-f00a-0155-4000-028d56e27999" />
	</SchemeComplexColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="a47725c3-03a0-4e29-9d3e-a96e280183d3" Name="ProjectQuality" Type="Reference(Typified) Null" ReferencedTable="f88137b6-5605-4e46-9408-5d74e0c312ef">
		<Description>Качество подготовки проекта</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="a47725c3-03a0-0029-4000-096e280183d3" Name="ProjectQualityID" Type="Guid Null" ReferencedColumn="f88137b6-5605-0146-4000-0d74e0c312ef" />
		<SchemeReferencingColumn ID="4ebb023b-16cf-4942-bede-e674e3667975" Name="ProjectQualityName" Type="String(Max) Null" ReferencedColumn="ca4950e0-4f74-4f94-aa05-ba13b6644738" />
	</SchemeComplexColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="922f5ee2-e4fd-4a28-8478-af5ce07b33a6" Name="AcceptanceProcedure" Type="Reference(Typified) Null" ReferencedTable="66508867-2104-4c64-b739-12f969f6dfe8">
		<Description>Порядок принятия</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="922f5ee2-e4fd-0028-4000-0f5ce07b33a6" Name="AcceptanceProcedureID" Type="Guid Null" ReferencedColumn="66508867-2104-0164-4000-02f969f6dfe8" />
		<SchemeReferencingColumn ID="8fcc607a-398f-42be-bdc9-0cb3fea282f2" Name="AcceptanceProcedureName" Type="String(Max) Null" ReferencedColumn="ed6d020b-023d-43af-abf4-c2ef98acf362" />
	</SchemeComplexColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="a2ae0ce2-46f6-40ed-9d27-5b222c4b24d3" Name="GriffMedo" Type="Reference(Typified) Null" ReferencedTable="3127c99d-c2e5-461b-8866-c6b63ce27eb2">
		<Description>Гриф (МЭДО)</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="a2ae0ce2-46f6-00ed-4000-0b222c4b24d3" Name="GriffMedoID" Type="Int16 Null" ReferencedColumn="daeedc2d-039e-4cbf-be5b-b66f665856d9" />
		<SchemeReferencingColumn ID="cfc01c2c-982e-4d22-b5ef-c0d559af7049" Name="GriffMedoIndex" Type="String(256) Null" ReferencedColumn="57df315d-e508-4422-86aa-211c21f84431" />
		<SchemeReferencingColumn ID="2349e2de-56b1-4cc9-bc9d-7271bffc4764" Name="GriffMedoName" Type="String(256) Null" ReferencedColumn="49a63b25-f1ec-4df3-bb3d-fadc8ba0e28b" />
	</SchemeComplexColumn>
	<SchemeComplexColumn Partition="c0aa4f8e-0bb0-4a9e-93c7-88507423a84c" ID="b4bf4262-a21d-4a14-86b1-f9c4d69aa4d5" Name="InDelo" Type="Reference(Typified) Null" ReferencedTable="27454eac-5316-4d34-baa6-31e0379447f6">
		<Description>Документ списан в дело</Description>
		<SchemeReferencingColumn IsSystem="true" IsPermanent="true" ID="b4bf4262-a21d-0014-4000-09c4d69aa4d5" Name="InDeloID" Type="Guid Null" ReferencedColumn="27454eac-5316-0134-4000-01e0379447f6" />
		<SchemeReferencingColumn ID="bdee167e-c2a0-46ac-b8eb-ca3e49e1a248" Name="InDeloIndex" Type="String(128) Null" ReferencedColumn="ecbfd5ce-7268-4bfc-98ec-d903d80124a6" />
		<SchemeReferencingColumn ID="14d25e27-9cd0-49f6-8aa4-4f5e9c7c9e99" Name="InDeloName" Type="String(512) Null" ReferencedColumn="3b87132b-cdeb-4f31-8ebf-4b60e3b85eb0" />
		<SchemeReferencingColumn ID="7aed4a86-1550-4498-a1c8-978d1da3a7ca" Name="InDeloDescription" Type="String(512) Null" ReferencedColumn="db1d439d-2e90-4233-8127-b2cdc6a027cf" />
	</SchemeComplexColumn>
	<Predicate Dbms="SqlServer">[RefDocID] IS NOT NULL</Predicate>
	<Predicate Dbms="PostgreSql">"RefDocID" IS NOT NULL</Predicate>
	<Predicate Dbms="SqlServer">[ReceiverRowID] IS NOT NULL</Predicate>
	<Predicate Dbms="PostgreSql">"ReceiverRowID" IS NOT NULL</Predicate>
	<Predicate Dbms="SqlServer">[CategoryID] IS NOT NULL</Predicate>
	<Predicate Dbms="PostgreSql">"CategoryID" IS NOT NULL</Predicate>
</SchemeTable>